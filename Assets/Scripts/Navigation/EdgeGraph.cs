using Unity.VisualScripting;
using UnityEngine;

public class EdgeGraph
{
    public float cost;
    public NodeGraph From;
    public NodeGraph To;

    public EdgeGraph(NodeGraph from, NodeGraph to)
    {
        From = from;
        To = to;
        cost = Vector2.Distance(From.pos, To.pos);
    }
}
