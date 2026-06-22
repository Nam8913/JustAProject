using UnityEngine;

public class DynamicVoronoiPoint : MonoBehaviour
{
    [SerializeField]
    VoronoiGraph graph;
    CircleCollider2D circleCollider;
    private Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    Vector2 GetMouseWorldPosition()
    {
        return PlayerInput.MousePosition;
    }

    public void SetGraph(VoronoiGraph test)
    {
        this.graph = test;
    }
}
