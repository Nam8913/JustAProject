using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoronoiGraph : MonoBehaviour
{
    public List<Vector2> voronoiGraphPoints = new List<Vector2>();
    //public List<Vector2> seeds = new List<Vector2>();
    private List<(Vector2, Vector2)> delaunayEdges = new List<(Vector2, Vector2)>();
    private List<(Vector2, Vector2)> voronoiEdges = new List<(Vector2, Vector2)>();

    public List<DynamicVoronoiPoint> dynamicPoints = new List<DynamicVoronoiPoint>();

    [Header("Settings")]
    public Color graphEdgeColor = Color.cyan;
    public Color delaunayEdgeColor = Color.green;
    public Color voronoiEdgeColor = Color.yellow;

    public bool renderGizmosGraphEdges = true;
    public bool renderGizmosDelaunayEdges = true;
    public bool renderGizmosVoronoiEdges = true;

    [Header("Ref")]
    public GameObject graphEdgesRenderer;
    public GameObject delaunayEdgesRenderer;
    public GameObject voronoiEdgesRenderer;

    [Header("Temp")]
    bool dirty = false;
    private List<Vector2> seeds = new List<Vector2>();

    [Header("Cache")]
    public List<Vector2> farPoints = new List<Vector2>();
    public List<Vector2> triCenters = new List<Vector2>();
    [Range(0.1f, 100f)]
    public float extendedLength = 1;

    public void MarkDirty(DynamicVoronoiPoint point)
    {
        dirty = true;
        // Cập nhật vị trí của điểm động trong danh sách seeds
        seeds[dynamicPoints.IndexOf(point)] = point.transform.position;
    }

    private GameObject CreateLineRenderer(string name, bool setParent = true)
    {
        GameObject lineObj = new GameObject(name);
        MeshRenderer meshRenderer = lineObj.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = lineObj.AddComponent<MeshFilter>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        if (setParent)
        {
            lineObj.transform.SetParent(this.transform);
        }
        return lineObj;
    }

    void RenderCheck()
    {
        if (graphEdgesRenderer == null)
        {
            graphEdgesRenderer = CreateLineRenderer("GraphEdges");
        }

        if (delaunayEdgesRenderer == null)
        {
            delaunayEdgesRenderer = CreateLineRenderer("DelaunayEdges");
        }

        if (voronoiEdgesRenderer == null)
        {
            voronoiEdgesRenderer = CreateLineRenderer("VoronoiEdges");
        }
    }

    void Awake()
    {
        RenderCheck();
    }

    void Start()
    {
        dirty = true;

        if (dynamicPoints.Count <= 1)
        {
            return;
        }

        foreach (var point in dynamicPoints)
        {
            point.SetGraph(this);
            seeds.Add(point.gameObject.transform.position);
        }


        if (voronoiGraphPoints.Count < 3)
        {
            Debug.LogError("Cần ít nhất 3 điểm để tạo biên cho đồ thị Voronoi!");
            return;
        }

        if (!AreAllPointsInsidePolygon(seeds, voronoiGraphPoints))
        {
            Debug.LogError("Vui lòng đảm bảo toàn bộ điểm (seeds) nằm trong đa giác!");
            return;
        }

        DrawGraph();
    }

    void DrawGraph()
    {
        // 1. Gọi thuật toán
        var triangles = DelaunayTriangulator.Triangulate(seeds);

        // 2. Trích xuất danh sách các cạnh duy nhất (các cặp điểm nối với nhau) và tính tâm đường tròn ngoại tiếp của mỗi tam giác
        HashSet<(int, int)> edges = new HashSet<(int, int)>();
        triCenters.Clear(); // Xóa danh sách cũ

        // LƯU Ý: Tạo một danh sách mới để lưu tam giác với chỉ số ổn định
        List<Triangle> stableTriangles = new List<Triangle>();

        foreach (var tri in triangles)
        {
            stableTriangles.Add(tri); // Lưu lại tam giác với chỉ số mới
            edges.Add((Mathf.Min(tri.a, tri.b), Mathf.Max(tri.a, tri.b)));
            edges.Add((Mathf.Min(tri.b, tri.c), Mathf.Max(tri.b, tri.c)));
            edges.Add((Mathf.Min(tri.c, tri.a), Mathf.Max(tri.c, tri.a)));
            Vector2 center = GetCircumcenter(seeds[tri.a], seeds[tri.b], seeds[tri.c]);
            triCenters.Add(center);
        }

        // 3. Vẽ các cạnh Delaunay (có thể dùng LineRenderer hoặc Debug.DrawLine)
        foreach (var edge in edges)
        {
            Vector3 p1 = seeds[edge.Item1];
            Vector3 p2 = seeds[edge.Item2];
            delaunayEdges.Add((p1, p2));
        }

        // 4. Xây dựng bản đồ để biết tam giác nào kề cạnh nào .Xây dựng edgeToTriangles từ stableTriangles (KHÔNG phải từ triangles gốc)


        // Tạo Dictionary lưu: (cạnh gồm 2 chỉ số đỉnh) -> danh sách các tam giác chứa cạnh đó
        Dictionary<(int, int), List<int>> edgeToTriangles = new Dictionary<(int, int), List<int>>();

        for (int i = 0; i < stableTriangles.Count; i++)
        {
            var tri = stableTriangles[i];
            AddEdgeToDict(tri.a, tri.b, i, edgeToTriangles);
            AddEdgeToDict(tri.b, tri.c, i, edgeToTriangles);
            AddEdgeToDict(tri.c, tri.a, i, edgeToTriangles);
        }

        // 5. Tạo các cạnh Voronoi từ các tam giác kề nhau

        List<Segment> rawVoronoiEdges = new List<Segment>();
        List<RawVoronoiSegment> rawVoronoiSegments = new List<RawVoronoiSegment>();

        foreach (var kvp in edgeToTriangles)
        {
            var triIndices = kvp.Value;
            var sharedEdge = kvp.Key; // (indexA, indexB)

            if (triIndices.Count == 2)
            {

                // Cạnh nội bộ: nối 2 circumcenter
                Vector2 p1 = triCenters[triIndices[0]];
                Vector2 p2 = triCenters[triIndices[1]];

                var seg = new Segment(p1, p2);
                rawVoronoiSegments.Add(new RawVoronoiSegment(seg, sharedEdge));
                rawVoronoiEdges.Add(seg);
            }
            if (triIndices.Count == 1)
            {
                // Cạnh biên: kéo dài từ circumcenter ra ngoài theo hướng vuông góc cạnh Delaunay
                int triIdx = triIndices[0];
                Vector2 circumCenter = triCenters[triIdx];

                Vector2 edgeA = seeds[sharedEdge.Item1];
                Vector2 edgeB = seeds[sharedEdge.Item2];

                // Hướng vuông góc với cạnh Delaunay
                Vector2 edgeMid = (edgeA + edgeB) * 0.5f;
                Vector2 edgeDir = (edgeB - edgeA).normalized;
                Vector2 perpDir = new Vector2(-edgeDir.y, edgeDir.x);

                // Đảm bảo hướng perpDir chỉ ra ngoài (ra xa circumcenter của tam giác còn lại)
                // Tức là hướng từ edgeMid ra ngoài tam giác
                var tri = triangles[triIdx]; // cần lưu lại triangles list
                Vector2 oppositeVertex = Vector2.zero;
                // Tìm đỉnh thứ 3 không thuộc cạnh biên
                foreach (int vIdx in new[] { tri.a, tri.b, tri.c })
                {
                    if (vIdx != sharedEdge.Item1 && vIdx != sharedEdge.Item2)
                    {
                        oppositeVertex = seeds[vIdx];
                        break;
                    }
                }

                // perpDir phải ngược chiều với (oppositeVertex - edgeMid)
                if (Vector2.Dot(perpDir, oppositeVertex - edgeMid) > 0)
                    perpDir = -perpDir;

                // Kéo dài đủ xa để clipping cắt lại
                float extendLength = extendedLength; // có thể điều chỉnh
                Vector2 farPoint = circumCenter + perpDir;

                if (!IsPointInPolygon(farPoint, voronoiGraphPoints))
                {
                    continue;
                }

                farPoints.Add(circumCenter + perpDir * extendLength); // Lưu lại để debug
                Segment seg = new Segment(circumCenter, circumCenter + perpDir * extendLength);
                rawVoronoiSegments.Add(new RawVoronoiSegment(seg, sharedEdge));

                rawVoronoiEdges.Add(new Segment(circumCenter, seg.end));
            }
        }

        HashSet<(int, int)> validPairs = new HashSet<(int, int)>();

        List<Segment> finalClippedEdges = new List<Segment>();
        foreach (var raw in rawVoronoiSegments)
        {
            var clippedParts = Voronoi.ClipSegmentByPolygon(raw.segment, voronoiGraphPoints);
            if (clippedParts.Count > 0)
            {
                validPairs.Add(raw.seedPair); // Đánh dấu cặp này hợp lệ
                finalClippedEdges.AddRange(clippedParts);
            }
        }
        foreach (var seg in finalClippedEdges)
        {
            voronoiEdges.Add((seg.start, seg.end));
        }

        delaunayEdges.Clear();
        foreach (var edge in edges) // edges là HashSet ban đầu
        {
            if (validPairs.Contains(edge))
            {
                Vector3 p1 = seeds[edge.Item1];
                Vector3 p2 = seeds[edge.Item2];
                delaunayEdges.Add((p1, p2));
            }
        }
    }
    #region Rendering
    void RendererGraphEdges(float width = 0.2f)
    {
        // Vẽ các cạnh biên của đồ thị Voronoi
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new();
        List<int> triangles = new();

        List<Vector2> uniquePoints = new List<Vector2>();
        uniquePoints.AddRange(voronoiGraphPoints);
        uniquePoints.Add(voronoiGraphPoints.First());

        for (int i = 0; i < uniquePoints.Count - 1; i++)
        {
            Vector2 a = uniquePoints[i];
            Vector2 b = uniquePoints[i + 1];

            AddLine(a, b, width, vertices, triangles);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        graphEdgesRenderer.GetComponent<MeshFilter>().mesh = mesh;
        graphEdgesRenderer.GetComponent<MeshRenderer>().material.color = graphEdgeColor;
    }

    void RendererDelaunayEdges(float width = 0.1f)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new();
        List<int> triangles = new();

        foreach (var edge in delaunayEdges)
        {
            AddLine(edge.Item1, edge.Item2, width, vertices, triangles);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        delaunayEdgesRenderer.GetComponent<MeshFilter>().mesh = mesh;
        delaunayEdgesRenderer.GetComponent<MeshRenderer>().material.color = delaunayEdgeColor;
    }

    void RendererVoronoiEdges(float width = 0.1f)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new();
        List<int> triangles = new();

        foreach (var edge in voronoiEdges)
        {
            AddLine(edge.Item1, edge.Item2, width, vertices, triangles);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        voronoiEdgesRenderer.GetComponent<MeshFilter>().mesh = mesh;
        voronoiEdgesRenderer.GetComponent<MeshRenderer>().material.color = voronoiEdgeColor;
    }
    #endregion
    void OnGUI()
    {
        if (dirty)
        {
            dirty = false;
            delaunayEdges.Clear();
            voronoiEdges.Clear();
            farPoints.Clear();
            DrawGraph(); // Recompute
            RendererGraphEdges();
            RendererDelaunayEdges();
            RendererVoronoiEdges();
        }

        if (!renderGizmosGraphEdges && graphEdgesRenderer.activeSelf != false)
        {
            graphEdgesRenderer.SetActive(false);
        }
        else if (renderGizmosGraphEdges && graphEdgesRenderer.activeSelf != true)
        {
            graphEdgesRenderer.SetActive(true);
        }

        if (!renderGizmosDelaunayEdges && delaunayEdgesRenderer.activeSelf != false)
        {
            delaunayEdgesRenderer.SetActive(false);
        }
        else if (renderGizmosDelaunayEdges && delaunayEdgesRenderer.activeSelf != true)
        {
            delaunayEdgesRenderer.SetActive(true);
        }

        if (!renderGizmosVoronoiEdges && voronoiEdgesRenderer.activeSelf != false)
        {
            voronoiEdgesRenderer.SetActive(false);
        }
        else if (renderGizmosVoronoiEdges && voronoiEdgesRenderer.activeSelf != true)
        {
            voronoiEdgesRenderer.SetActive(true);
        }
    }

    void OnDrawGizmos()
    {
        foreach (var point in voronoiGraphPoints)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 0.1f);
        }

        if (renderGizmosGraphEdges)
        {
            List<(Vector2, Vector2)> voronoiGraphEdges = new List<(Vector2, Vector2)>();
            int n = voronoiGraphPoints.Count;
            for (int i = 0; i < n; i++)
            {
                Vector2 p1 = voronoiGraphPoints[i];
                Vector2 p2 = voronoiGraphPoints[(i + 1) % n];
                voronoiGraphEdges.Add((p1, p2));
            }
            foreach (var edge in voronoiGraphEdges)
            {
                Gizmos.color = graphEdgeColor;
                Gizmos.DrawLine(new Vector3(edge.Item1.x, edge.Item1.y, 0), new Vector3(edge.Item2.x, edge.Item2.y, 0));
            }
        }

        if (renderGizmosDelaunayEdges)
        {
            foreach (var edge in delaunayEdges)
            {
                Gizmos.color = delaunayEdgeColor;
                Gizmos.DrawLine(new Vector3(edge.Item1.x, edge.Item1.y, 0), new Vector3(edge.Item2.x, edge.Item2.y, 0));
            }
        }

        if (renderGizmosVoronoiEdges)
        {
            foreach (var edge in voronoiEdges)
            {
                Gizmos.color = voronoiEdgeColor;
                Gizmos.DrawLine(new Vector3(edge.Item1.x, edge.Item1.y, 0), new Vector3(edge.Item2.x, edge.Item2.y, 0));
            }
        }

        // 4. Vẽ các điểm seeds
        // foreach (var s in seeds)
        // {
        //     Debug.DrawLine(new Vector3(s.x - 0.2f, s.y - 0.2f), new Vector3(s.x + 0.2f, s.y + 0.2f), Color.red);
        //     Debug.DrawLine(new Vector3(s.x - 0.2f, s.y + 0.2f), new Vector3(s.x + 0.2f, s.y - 0.2f), Color.red);
        // }

        foreach (var point in farPoints)
        {
            // Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 0.1f);
            Debug.DrawLine(new Vector3(point.x - 0.2f, point.y - 0.2f), new Vector3(point.x + 0.2f, point.y + 0.2f), Color.magenta);
            Debug.DrawLine(new Vector3(point.x - 0.2f, point.y + 0.2f), new Vector3(point.x + 0.2f, point.y - 0.2f), Color.magenta);
        }

        foreach (var center in triCenters)
        {
            Debug.DrawLine(new Vector3(center.x - 0.2f, center.y - 0.2f), new Vector3(center.x + 0.2f, center.y + 0.2f), Color.black);
            Debug.DrawLine(new Vector3(center.x - 0.2f, center.y + 0.2f), new Vector3(center.x + 0.2f, center.y - 0.2f), Color.black);
        }
    }

    void AddEdgeToDict(int i, int j, int triIdx, Dictionary<(int, int), List<int>> dict)
    {
        if (i > j) (i, j) = (j, i); // luôn để (min, max)
        var key = (i, j);
        if (!dict.ContainsKey(key)) dict[key] = new List<int>();
        dict[key].Add(triIdx);
    }

    Vector2 GetCircumcenter(Vector2 A, Vector2 B, Vector2 C)
    {
        float d = 2f * (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y));
        if (Mathf.Abs(d) < 1e-6f) return (A + B + C) / 3f; // tránh lỗi chia 0

        float ux = ((A.x * A.x + A.y * A.y) * (B.y - C.y) +
                    (B.x * B.x + B.y * B.y) * (C.y - A.y) +
                    (C.x * C.x + C.y * C.y) * (A.y - B.y)) / d;

        float uy = ((A.x * A.x + A.y * A.y) * (C.x - B.x) +
                    (B.x * B.x + B.y * B.y) * (A.x - C.x) +
                    (C.x * C.x + C.y * C.y) * (B.x - A.x)) / d;

        return new Vector2(ux, uy);
    }

    /// <summary>
    /// Kiểm tra xem TẤT CẢ các điểm trong danh sách có nằm trong (hoặc trên biên) 
    /// của một đa giác được xác định bởi danh sách các đỉnh hay không.
    /// </summary>
    /// <param name="points">Danh sách các điểm cần kiểm tra.</param>
    /// <param name="polygon">Danh sách các đỉnh của đa giác (theo chiều kim đồng hồ hoặc ngược lại).</param>
    /// <returns>True nếu tất cả điểm đều nằm trong đa giác, ngược lại False.</returns>
    public bool AreAllPointsInsidePolygon(List<Vector2> points, List<Vector2> polygon)
    {
        // Kiểm tra đa giác hợp lệ (ít nhất 3 đỉnh)
        if (polygon == null || polygon.Count < 3)
        {
            Debug.LogError("Đa giác phải có ít nhất 3 đỉnh!");
            return false;
        }

        // Nếu danh sách điểm rỗng -> trả về true (không có điểm nào nằm ngoài)
        if (points == null || points.Count == 0)
            return true;

        // Duyệt tất cả điểm, nếu có 1 điểm nằm ngoài -> trả về false ngay lập tức
        foreach (Vector2 point in points)
        {
            if (!IsPointInPolygon(point, polygon))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Kiểm tra một điểm duy nhất có nằm trong đa giác hay không.
    /// Sử dụng thuật toán Ray Casting (Even-Odd Rule).
    /// </summary>
    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int polygonLength = polygon.Count;
        bool inside = false;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];

            // Điều kiện 1: Điểm P phải nằm trong khoảng tung độ của cạnh (vi, vj)
            // Sử dụng dấu != để bỏ qua trường hợp cạnh nằm ngang (giúp xử lý đỉnh)
            bool intersect = ((vi.y > point.y) != (vj.y > point.y));

            if (intersect)
            {
                // Điều kiện 2: Tính hoành độ giao điểm của tia (từ P sang phải) với cạnh
                // Công thức nội suy: x = vj.x + (vi.x - vj.x) * (point.y - vj.y) / (vi.y - vj.y)
                float intersectionX = (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x;

                // Nếu giao điểm nằm bên phải điểm P -> đếm là 1 lần cắt
                if (point.x < intersectionX)
                    inside = !inside;
            }
        }

        return inside;
    }

    public void AddLine(
        Vector2 a,
        Vector2 b,
        float width,
        List<Vector3> vertices,
        List<int> triangles)
    {
        int startIndex = vertices.Count;

        Vector2 dir = (b - a).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);
        Vector2 offset = normal * width * 0.5f;

        vertices.Add(a + offset);
        vertices.Add(b + offset);
        vertices.Add(a - offset);
        vertices.Add(b - offset);

        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);

        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 3);
    }

    public class RawVoronoiSegment
    {
        public Segment segment;
        public (int, int) seedPair; // cặp seed (a,b) đã chuẩn hóa
        public RawVoronoiSegment(Segment seg, (int, int) pair)
        {
            segment = seg;
            seedPair = pair;
        }
    }

    public struct Triangle
    {
        public int a, b, c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public struct Segment
    {
        public Vector2 start;
        public Vector2 end;
        public Segment(Vector2 s, Vector2 e) { start = s; end = e; }
    }

    public static class Voronoi
    {

        // -------------------- Các hàm hình học phụ trợ --------------------

        /// <summary>
        /// Kiểm tra điểm có nằm trong đa giác (dùng Ray Casting).
        /// </summary>
        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            int n = polygon.Count;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Vector2 vi = polygon[i];
                Vector2 vj = polygon[j];

                bool intersect = ((vi.y > point.y) != (vj.y > point.y));
                if (intersect)
                {
                    float xIntersect = (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x;
                    if (point.x < xIntersect)
                        inside = !inside;
                }
            }
            return inside;
        }
        public static List<Segment> ClipSegmentByPolygon(Segment seg, List<Vector2> polygon)
        {
            List<Segment> results = new List<Segment>();
            if (polygon.Count < 3) return results;

            Vector2 A = seg.start;
            Vector2 B = seg.end;
            Vector2 dir = B - A;
            float len = dir.magnitude;
            if (len < Mathf.Epsilon) // đoạn thẳng có độ dài 0
            {
                if (IsPointInPolygon(A, polygon))
                    results.Add(new Segment(A, A));
                return results;
            }

            // Bước 1: Thu thập tất cả tham số t tại các điểm giao với cạnh đa giác
            List<float> tValues = new List<float> { 0f, 1f }; // luôn có hai đầu mút

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p1 = polygon[i];
                Vector2 p2 = polygon[(i + 1) % polygon.Count];
                Vector2 edge = p2 - p1;

                // Tính giao điểm giữa đoạn thẳng (A->B) và cạnh (p1->p2)
                // Giải hệ: A + t*dir = p1 + u*edge
                float det = dir.x * edge.y - dir.y * edge.x;
                if (Mathf.Abs(det) < 1e-6f) continue; // song song hoặc trùng

                float t = ((p1.x - A.x) * edge.y - (p1.y - A.y) * edge.x) / det;
                float u = ((p1.x - A.x) * dir.y - (p1.y - A.y) * dir.x) / det;

                // Giao điểm nằm trong cả hai đoạn (từ 0 đến 1, và u từ 0 đến 1)
                if (t > 0f && t < 1f && u >= 0f && u <= 1f)
                {
                    tValues.Add(t);
                }
            }

            // Sắp xếp và loại bỏ các giá trị trùng lặp
            tValues.Sort();
            for (int i = tValues.Count - 1; i > 0; i--)
            {
                if (Mathf.Abs(tValues[i] - tValues[i - 1]) < 1e-6f)
                    tValues.RemoveAt(i);
            }

            // Bước 2: Với mỗi khoảng giữa hai t liên tiếp, kiểm tra điểm giữa có nằm trong đa giác không
            for (int i = 0; i < tValues.Count - 1; i++)
            {
                float t1 = tValues[i];
                float t2 = tValues[i + 1];
                float mid = (t1 + t2) * 0.5f;
                Vector2 midPoint = A + dir * mid;

                if (IsPointInPolygon(midPoint, polygon))
                {
                    // Đoạn con từ t1 đến t2 hoàn toàn nằm trong (hoặc trên biên)
                    Vector2 start = A + dir * t1;
                    Vector2 end = A + dir * t2;
                    results.Add(new Segment(start, end));
                }
            }

            return results;
        }

        /// <summary>
        /// Áp dụng clipping cho toàn bộ danh sách cạnh của đồ thị Voronoi.
        /// Trả về danh sách cạnh mới đã bị cắt gọn trong đa giác.
        /// </summary>
        public static List<Segment> ClipVoronoiEdges(List<Segment> originalEdges, List<Vector2> clipPolygon)
        {
            List<Segment> clipped = new List<Segment>();
            foreach (var edge in originalEdges)
            {
                var parts = ClipSegmentByPolygon(edge, clipPolygon);
                clipped.AddRange(parts);
            }
            return clipped;
        }
    }

    public static class DelaunayTriangulator
    {
        // Cấu trúc tam giác lưu 3 chỉ số đỉnh


        // Hàm chính: trả về danh sách tam giác từ danh sách điểm
        public static List<Triangle> Triangulate(List<Vector2> points)
        {
            // 1. Tạo một tam giác siêu lớn bao phủ tất cả điểm
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (var p in points)
            {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            float dx = maxX - minX;
            float dy = maxY - minY;
            float delta = Mathf.Max(dx, dy) * 2f; // đủ lớn để bao

            Vector2 p1 = new Vector2(minX - delta, minY - delta);
            Vector2 p2 = new Vector2(minX + delta, minY - delta);
            Vector2 p3 = new Vector2((minX + maxX) / 2f, maxY + delta);

            // Thêm 3 đỉnh này vào danh sách (tạm thời)
            List<Vector2> allPoints = new List<Vector2>(points);
            int superIndex1 = allPoints.Count;
            int superIndex2 = allPoints.Count + 1;
            int superIndex3 = allPoints.Count + 2;
            allPoints.Add(p1);
            allPoints.Add(p2);
            allPoints.Add(p3);

            // Khởi tạo danh sách tam giác với tam giác siêu lớn
            List<Triangle> triangles = new List<Triangle>();
            triangles.Add(new Triangle(superIndex1, superIndex2, superIndex3));

            // 2. Chèn từng điểm vào
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];
                List<Triangle> badTriangles = new List<Triangle>();

                // Tìm tam giác xấu (chứa điểm trong đường tròn ngoại tiếp)
                foreach (var tri in triangles)
                {
                    if (IsPointInsideCircumcircle(point, allPoints[tri.a], allPoints[tri.b], allPoints[tri.c]))
                    {
                        badTriangles.Add(tri);
                    }
                }

                // Xác định các cạnh biên của vùng đa giác (cạnh chỉ thuộc 1 tam giác xấu)
                HashSet<Edge> edges = new HashSet<Edge>();
                foreach (var tri in badTriangles)
                {
                    // 3 cạnh của tam giác
                    Edge e1 = new Edge(tri.a, tri.b);
                    Edge e2 = new Edge(tri.b, tri.c);
                    Edge e3 = new Edge(tri.c, tri.a);

                    // Nếu cạnh này xuất hiện trong 2 tam giác xấu, thì đó là cạnh trong, bỏ qua
                    // Ngược lại, nếu chỉ xuất hiện 1 lần, đó là cạnh biên của đa giác
                    AddEdgeIfUnique(e1, edges);
                    AddEdgeIfUnique(e2, edges);
                    AddEdgeIfUnique(e3, edges);
                }

                // Xóa các tam giác xấu khỏi danh sách
                foreach (var tri in badTriangles)
                    triangles.Remove(tri);

                // Tạo tam giác mới nối điểm i với các cạnh biên
                foreach (var edge in edges)
                {
                    triangles.Add(new Triangle(i, edge.a, edge.b));
                }
            }

            // 3. Loại bỏ tam giác nào chứa đỉnh siêu lớn
            triangles.RemoveAll(tri => tri.a >= points.Count || tri.b >= points.Count || tri.c >= points.Count);

            return triangles;
        }

        // Kiểm tra điểm p có nằm trong đường tròn ngoại tiếp của tam giác (a,b,c) không
        private static bool IsPointInsideCircumcircle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float ax = a.x - p.x; float ay = a.y - p.y;
            float bx = b.x - p.x; float by = b.y - p.y;
            float cx = c.x - p.x; float cy = c.y - p.y;

            float det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                    - (bx * bx + by * by) * (ax * cy - cx * ay)
                    + (cx * cx + cy * cy) * (ax * by - bx * ay);

            // Với các điểm không thẳng hàng, det > 0 có nghĩa là p nằm trong đường tròn (theo thứ tự ngược chiều kim đồng hồ)
            // Ta kiểm tra hướng của tam giác, nếu không cùng hướng thì đảo dấu
            if (Cross2D(a, b, c) < 0)
                det = -det;

            return det > 0f;
        }

        // Tích có hướng (cross product) của vector AB và AC
        private static float Cross2D(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        // Thêm cạnh vào HashSet, nếu đã tồn tại thì xóa (vì xuất hiện 2 lần)
        private static void AddEdgeIfUnique(Edge edge, HashSet<Edge> edges)
        {
            // Chuẩn hóa thứ tự đỉnh để so sánh
            if (edge.a > edge.b) (edge.a, edge.b) = (edge.b, edge.a);

            if (edges.Contains(edge))
                edges.Remove(edge);
            else
                edges.Add(edge);
        }

        // Ghi đè Equals và GetHashCode cho Edge để HashSet hoạt động đúng
        private struct Edge : System.IEquatable<Edge>
        {
            public int a, b;
            public Edge(int a, int b) { this.a = a; this.b = b; }

            public bool Equals(Edge other) => (a == other.a && b == other.b);
            public override int GetHashCode() => a * 1000003 + b;
        }
    }
}