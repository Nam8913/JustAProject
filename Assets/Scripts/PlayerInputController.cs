using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] 
    public InputActionAsset inputActions; // kéo file .inputactions vào đây
    [SerializeField] 
    [ReadOnly]
    public InputActionMap playerActionMap;
    [SerializeField] 
    [ReadOnly]
    public InputActionMap uiActionMap;

    [SerializeField]
    private Transform _playerTransform;
    [SerializeField]
    private Rigidbody2D rgb2d;

    [SerializeField]
    private float _moveSpeed = 5f;

    [SerializeField]
    [TextArea(3, 10)]
    private string debugInfo = string.Empty;

    static PlayerInputController _instance;
    static readonly object _lock = new object();
    public static PlayerInputController Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<PlayerInputController>();
                        if (_instance == null)
                        {
                            GameObject obj = new GameObject("PlayerInputController");
                            GameObject.DontDestroyOnLoad(obj);
                            _instance = obj.AddComponent<PlayerInputController>();
                        }
                    }
                }
            }
            return _instance;
        }
    }

    public void SetTransform(Transform playerTransform)
    {
        _playerTransform = playerTransform;

        if (rgb2d == null)
        {
            rgb2d = _playerTransform.GetComponent<Rigidbody2D>();
            if (rgb2d == null)
            {
                rgb2d = _playerTransform.gameObject.AddComponent<Rigidbody2D>();
                rgb2d.gravityScale = 0f;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (inputActions != null)
        {
            playerActionMap = inputActions.FindActionMap("Player");
            uiActionMap = inputActions.FindActionMap("UI");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_playerTransform != null && playerActionMap != null)
        {
            Vector2 moveInput = playerActionMap.FindAction("Move")?.ReadValue<Vector2>() ?? Vector2.zero;
            Vector2 moveVelocity = moveInput * _moveSpeed;
            rgb2d.linearVelocity = moveVelocity;
        }
    }

    void OnDestroy()
    {
        _instance = null;
    }
}