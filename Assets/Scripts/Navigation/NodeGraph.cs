using System.Collections.Generic;
using UnityEngine;

public sealed class NodeGraph
{
    public Vector2 pos;
    public bool walkable;
    public float moveCost = 1f;
    public bool debugOpen;
    public bool debugClosed;
    public bool debugPath;

    public float gCost;// Distance from the start node
    public float hCost;// Distance from the end node
    public float fCost => gCost + hCost;

    public NodeGraph parent;
    public List<EdgeGraph> edges;

    public void FindEdges4Dir(GraphPathfinder graphPathfinder)
    {
        var directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        foreach (var dir in directions)
        {
            var neighborPos = pos + dir;
            var neighborNode = graphPathfinder.GetNode(neighborPos);
            if (neighborNode != null && neighborNode.walkable)
            {
                var edge = new EdgeGraph(this, neighborNode);
                edges.Add(edge);
            }
        }
    }

    public void FindEdges8Dir(GraphPathfinder graphPathfinder)
    {
        var directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
            Vector2.up + Vector2.left,
            Vector2.up + Vector2.right,
            Vector2.down + Vector2.left,
            Vector2.down + Vector2.right
        };

        foreach (var dir in directions)
        {
            var neighborPos = pos + dir;
            var neighborNode = graphPathfinder.GetNode(neighborPos);
            if (neighborNode != null && neighborNode.walkable)
            {
                var edge = new EdgeGraph(this, neighborNode);
                edges.Add(edge);
            }
        }
    }
}