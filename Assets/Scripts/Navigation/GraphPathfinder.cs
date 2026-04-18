using System.Collections.Generic;
using UnityEngine;

public sealed class GraphPathfinder
{
    private Dictionary<Vector2,NodeGraph> graph;


    public GraphPathfinder()
    {
        graph = new Dictionary<Vector2, NodeGraph>();
    }

    public Dictionary<Vector2, NodeGraph> Graph
    {
        get => graph;
    }

    public void AddNode(NodeGraph node)
    {
        if (!graph.ContainsKey(node.pos))
        {
            graph.Add(node.pos, node);
        }
    }

    public void AddNodeByVector2(Vector2 pos, bool walkable)
    {
        AddNodeByVector2(pos, walkable, 1f);
    }

    public void AddNodeByVector2(Vector2 pos, bool walkable, float moveCost)
    {
        if (!graph.ContainsKey(pos))
        {
            var node = new NodeGraph
            {
                pos = pos,
                walkable = walkable,
                moveCost = moveCost,
                edges = new List<EdgeGraph>()
            };
            graph.Add(pos, node);
        }
    }

    public void RemoveNode(Vector2 pos)
    {
        if (graph.ContainsKey(pos))
        {
            graph.Remove(pos);
        }
    }

    public NodeGraph GetNode(Vector2 pos)
    {
        if (graph.ContainsKey(pos))
        {
            return graph[pos];
        }
        return null;
    }

    public List<EdgeGraph> GetEdgesOfNode(Vector2 pos)
    {
        if (graph.ContainsKey(pos))
        {
            return graph[pos].edges;
        }
        return null;
    }
}