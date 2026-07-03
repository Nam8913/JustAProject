using System.Collections.Generic;
using UnityEngine;

public class DynamicVoronoiPoint : MonoBehaviour
{
    [SerializeField]
    VoronoiGraph graph;
    CircleCollider2D circleCollider;
    PolygonCollider2D polyCollider;

    [Header("Cell Info")]
    [SerializeField]
    [ReadOnly]
    private float area;
    [ReadOnly]
    public Vector2 seedPosition;
    public List<Vector2> cellPolygon = new List<Vector2>(); // các đỉnh của cell theo thứ tự
    public Color color;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Cache")]
    private Vector3 offset;
    void Awake()
    {
        color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
         if(polyCollider == null)
        {
            GameObject polyColliderObj = new GameObject("PolyCollider");
            polyColliderObj.transform.SetParent(transform);
            polyColliderObj.transform.localPosition = Vector3.zero;
            polyColliderObj.transform.localRotation = Quaternion.identity;
            polyColliderObj.transform.localScale = Vector3.one;
            polyCollider = polyColliderObj.AddComponent<PolygonCollider2D>();

            polyCollider.pathCount = 0;
        }
    }
    void Start()
    {
        circleCollider = GetComponent<CircleCollider2D>();
        if(circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnMouseDown()
    {
        offset = transform.position - (Vector3)GetMouseWorldPosition();
    }

    void OnMouseDrag()
    {
        transform.position = (Vector3)GetMouseWorldPosition() + offset;
        if(graph != null)
        {
            graph.MarkDirty(this);
        }
    }

    /// <summary>
    /// Được gọi bởi VoronoiGraph sau khi tính xong cell
    /// </summary>
    public void SetCell(List<Vector2> worldPolygon, Vector2 seed)
    {
        cellPolygon = worldPolygon;
        seedPosition = seed;
        area = SignedPolygonArea(cellPolygon);

        UpdateCollider();
    }

    void UpdateCollider()
    {
        polyCollider.pathCount = 1;

        if (cellPolygon == null || cellPolygon.Count < 3)
            return;

        // PolygonCollider2D dùng local space
        // Nếu GameObject ở gốc tọa độ thì world == local
        // Nếu không, cần convert:
        Vector2[] localPts = new Vector2[cellPolygon.Count];
        for (int i = 0; i < cellPolygon.Count; i++)
        {
            localPts[i] = transform.InverseTransformPoint(cellPolygon[i]);
        }

        polyCollider.SetPath(0, localPts);
    }

    // Khi điểm di chuyển, cập nhật collider
    void OnDrawGizmos()
    {
        // if (cellPolygon == null || cellPolygon.Count < 3) return;

        // Gizmos.color = color;
        // for (int i = 0; i < cellPolygon.Count; i++)
        // {
        //     Vector3 a = cellPolygon[i];
        //     Vector3 b = cellPolygon[(i + 1) % cellPolygon.Count];
        //     Gizmos.DrawLine(a, b);
        // }
    }

    Vector2 GetMouseWorldPosition()
    {
        return PlayerInput.MousePosition;
    }

    public void SetGraph(VoronoiGraph test)
    {
        this.graph = test;
    }

    /// <summary>
    /// Tính diện tích đa giác bằng Shoelace formula.
    /// Đỉnh có thể theo chiều CW hoặc CCW đều được (kết quả lấy abs).
    /// </summary>
    public static float PolygonArea(IList<Vector2> points)
    {
        int n = points.Count;
        if (n < 3) return 0f;

        float sum = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % n]; // wrap về đỉnh đầu
            sum += (p1.x * p2.y) - (p2.x * p1.y);
        }

        return Mathf.Abs(sum) * 0.5f;
    }

    /// <summary>
    /// Trả về diện tích có dấu - dùng để biết chiều winding.
    /// Dương = CCW (counter-clockwise), Âm = CW (clockwise)
    /// </summary>
    public static float SignedPolygonArea(IList<Vector2> points)
    {
        int n = points.Count;
        float sum = 0f;
        for (int i = 0; i < n; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % n];
            sum += (p1.x * p2.y) - (p2.x * p1.y);
        }
        return sum * 0.5f;
    }

    public static bool IsClockwise(IList<Vector2> points)
        => SignedPolygonArea(points) < 0f;
}
