using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();

    [Header("Refill Settings")]
    [Tooltip("Số object tối đa được Instantiate trong 1 frame khi refill pool.")]
    [SerializeField] private int loadMaxObjectPerFrame = 10;

    [Tooltip("Ngân sách thời gian (ms) tối đa dành cho việc refill mỗi frame. " +
             "Dùng để bảo vệ frame khi prefab nặng (nhiều component, mesh phức tạp...). " +
             "Đặt <= 0 để bỏ qua, chỉ dùng giới hạn theo số lượng.")]
    [SerializeField] private float loadMaxMillisecondsPerFrame = 2f;

    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    private Dictionary<string, GameObject> _prefabLookup;

    // Số lượng object cần Instantiate thêm để bù lại cho pool sau khi bị lấy ra (GetFromPool).
    private readonly Dictionary<string, int> _pendingRefill = new Dictionary<string, int>();

    // Cờ khóa: đảm bảo CHỈ MỘT coroutine refill chạy tại một thời điểm.
    // Đây là điểm mấu chốt để tránh race condition kiểu reentrancy như bản async void Update() cũ.
    private Coroutine _refillRoutine;

    private readonly System.Diagnostics.Stopwatch _frameStopwatch = new System.Diagnostics.Stopwatch();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        _prefabLookup = new Dictionary<string, GameObject>();

        foreach (Pool pool in pools)
        {
            RegisterPool(pool.tag, pool.prefab, pool.size);
        }
    }

    private void RegisterPool(string tag, GameObject prefab, int size)
    {
        if (_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' đã tồn tại, bỏ qua đăng ký trùng.");
            return;
        }

        Queue<GameObject> objectQueue = new Queue<GameObject>();
        GameObject parent = HolderManager.CreateNewHolderObject($"Pool_{tag}").gameObject;

        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(prefab, parent.transform);
            obj.SetActive(false);
            objectQueue.Enqueue(obj);
        }

        _poolDictionary.Add(tag, objectQueue);
        _prefabLookup.Add(tag, prefab);
    }

    /// <summary>
    /// Đăng ký nhu cầu cần refill thêm 1 object cho tag này, và tự khởi động
    /// coroutine refill nếu chưa có coroutine nào đang chạy.
    /// KHÔNG được gọi trực tiếp Instantiate ở đây để tránh spike frame khi có nhiều request dồn dập.
    /// </summary>
    private void RequestRefill(string tag, int amount = 1)
    {
        if (_pendingRefill.ContainsKey(tag))
            _pendingRefill[tag] += amount;
        else
            _pendingRefill[tag] = amount;

        // Chỉ start coroutine mới nếu chưa có cái nào đang chạy.
        // Đây là phần thay thế cho async void Update() lỗi ở bản cũ.
        if (_refillRoutine == null)
        {
            _refillRoutine = StartCoroutine(RefillRoutine());
        }
    }

    private IEnumerator RefillRoutine()
    {
        int loadCountThisFrame = 0;
        _frameStopwatch.Restart();

        while (_pendingRefill.Count > 0)
        {
            // Snapshot danh sách tag để tránh lỗi "Collection was modified" khi vừa duyệt vừa Remove.
            var tags = new List<string>(_pendingRefill.Keys);

            foreach (var tag in tags)
            {
                // Nếu tag đã bị remove bởi vòng lặp trước đó trong cùng frame thì bỏ qua.
                if (!_pendingRefill.ContainsKey(tag))
                    continue;

                bool overCountLimit = loadCountThisFrame >= loadMaxObjectPerFrame;
                bool overTimeLimit = loadMaxMillisecondsPerFrame > 0f
                    && _frameStopwatch.Elapsed.TotalMilliseconds >= loadMaxMillisecondsPerFrame;

                if (overCountLimit || overTimeLimit)
                {
                    yield return null; // đợi sang frame kế tiếp
                    loadCountThisFrame = 0;
                    _frameStopwatch.Restart();
                }

                if (!_prefabLookup.TryGetValue(tag, out GameObject prefab))
                {
                    Debug.LogWarning($"Không tìm thấy prefab cho tag '{tag}', huỷ refill tag này.");
                    _pendingRefill.Remove(tag);
                    continue;
                }

                GameObject holder = HolderManager.GetHolderObject($"Pool_{tag}").gameObject;
                GameObject obj = Instantiate(prefab, holder != null ? holder.transform : transform);
                obj.SetActive(false);

                if (!_poolDictionary.ContainsKey(tag))
                    _poolDictionary[tag] = new Queue<GameObject>();

                _poolDictionary[tag].Enqueue(obj);

                _pendingRefill[tag]--;
                if (_pendingRefill[tag] <= 0)
                    _pendingRefill.Remove(tag);

                loadCountThisFrame++;
            }
        }

        // Báo hiệu đã xử lý xong toàn bộ hàng đợi -> request tiếp theo được phép start coroutine mới.
        _refillRoutine = null;
    }

    /// <summary>
    /// Lấy object ra dùng TẠM THỜI — object này được kỳ vọng sẽ được trả lại pool
    /// sau đó bằng ReturnToPool (đạn bắn xong, effect chạy xong...).
    /// KHÔNG kích hoạt refill vì vòng đời object vẫn khép kín trong pool
    /// (lấy ra bao nhiêu, sớm muộn cũng trả lại bấy nhiêu).
    /// Nếu hết hàng sẵn có, tự mở rộng thêm 1 object mới và object đó cũng sẽ
    /// được nhập vào vòng tuần hoàn của pool khi ReturnToPool được gọi.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform newParent = null)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' không tồn tại.");
            return null;
        }

        Queue<GameObject> queue = _poolDictionary[tag];

        // Không refill: object này rồi sẽ được ReturnToPool trả lại, không mất sĩ số của pool.
        GameObject objectToSpawn = queue.Count > 0
            ? queue.Dequeue()
            : Instantiate(_prefabLookup[tag]);

        ActivateAndPrepare(objectToSpawn, position, rotation, newParent == null ? HolderManager.GetHolderObject($"Pool_{tag}").transform : newParent);
        return objectToSpawn;
    }

    /// <summary>
    /// Lấy object ra khỏi pool VĨNH VIỄN (không có ý định trả lại bằng ReturnToPool
    /// — ví dụ: object được giao cho hệ thống khác quản lý, hoặc sẽ tự Destroy).
    /// Vì object không quay lại, pool sẽ tự đăng ký Instantiate thêm 1 object mới
    /// (spread qua nhiều frame trong RefillRoutine) để bù lại sĩ số ban đầu.
    /// </summary>
    public GameObject GetFromPool(string tag, Transform parent = null)
    {
        return GetFromPool(tag, Vector3.zero, Quaternion.identity, parent);
    }

    public GameObject GetFromPool(string tag, Vector3 position, Quaternion rotation, Transform newParent = null)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' không tồn tại.");
            return null;
        }

        Queue<GameObject> queue = _poolDictionary[tag];
        GameObject objectToGet;

        if (queue.Count > 0)
        {
            objectToGet = queue.Dequeue();

            // Object này rời khỏi pool vĩnh viễn -> đăng ký refill để bù lại (không Instantiate ngay ở đây).
            RequestRefill(tag, 1);
        }
        else
        {
            // Hết hàng sẵn có -> tạo object mới ngay để không làm caller phải chờ.
            // Object này chưa từng thuộc pool nên không cần RequestRefill.
            objectToGet = Instantiate(_prefabLookup[tag]);
        }

        ActivateAndPrepare(objectToGet, position, rotation, newParent);
        return objectToGet;
    }

    private void ActivateAndPrepare(GameObject obj, Vector3 position, Quaternion rotation, Transform newParent)
    {
        obj.SetActive(true);
        obj.transform.SetPositionAndRotation(position, rotation);

        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnSpawn();

        obj.transform.SetParent(newParent);
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (obj.TryGetComponent(out IPoolable poolable))
            poolable.OnDespawn();

        obj.SetActive(false);

        if (!_poolDictionary.ContainsKey(tag))
            _poolDictionary[tag] = new Queue<GameObject>();

        // Đặt parent về holder của pool để giữ hierarchy gọn gàng, tránh clutter trong scene.
        obj.transform.SetParent(HolderManager.GetHolderObject($"Pool_{tag}").transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        _poolDictionary[tag].Enqueue(obj);
    }

    public static void CreateNewPool(string tag, GameObject prefab, int size)
    {
        if (Instance == null)
        {
            Debug.LogError("PoolManager chưa được khởi tạo.");
            return;
        }

        if (Instance._poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' đã tồn tại.");
            return;
        }

        prefab.transform.SetParent(HolderManager.GetHolderObject("TrashHolder").transform);
        Instance.RegisterPool(tag, prefab, size);
    }

    public static void CreatePoolManager()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject poolManagerObj = new GameObject("PoolManager");
        poolManagerObj.AddComponent<PoolManager>();
        DontDestroyOnLoad(poolManagerObj);
    }
}