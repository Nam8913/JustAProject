using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    World world;
    private HPAPathfinder hpaPathfinder;
    private List<Vector2> path;

    public Vector2Int start;
    public Vector2Int end;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        world = new World("testWorld", 12345);
        DebugScripts debugScripts = this.gameObject.AddComponent<DebugScripts>();

        hpaPathfinder = new HPAPathfinder();
        hpaPathfinder.AutoReleaseUnusedChunkGraphs = true;

        Creature demoCreature = ThingHandler.CreateThing<Creature>();
            demoCreature.transform.position = new Vector2(start.x, start.y);
        

        //path = hpaPathfinder.GetPathFromTo(demoCreature.WorldPosition, new Vector2(end.x, end.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // void OnDrawGizmos()
    // {
    //     if (hpaPathfinder == null)
    //     {
    //         return;
    //     }

    //     foreach(var chunk in WorldHandler.GetActiveChunks().Values)
    //     {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawWireCube(chunk.transform.position + new Vector3(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f, 0f), new Vector3(LocalTestValue.tilesPerChunk, LocalTestValue.tilesPerChunk, 0f));
    //     }

    //     DrawGraphForChunk(hpaPathfinder.ChunkGraph, 0.22f, 0.04f, true);

    //     foreach (var chunkGraphPair in hpaPathfinder.TileGraphsByChunk)
    //     {
    //         DrawGraphForTiles(chunkGraphPair.Value, 0.12f, 0.02f, false);
    //     }

    //     //DrawGatewayPoints();

    //     DrawPath();
    // }

    

    public void DoSomeThing()
    {
        if (hpaPathfinder == null)
        {
            hpaPathfinder = new HPAPathfinder();
        }

        path = hpaPathfinder.GetPathFromTo(new Vector2(start.x, start.y), new Vector2(end.x, end.y));
    }

    private void DrawGraphForChunk(GraphPathfinder chunkGraph, float v1, float v2, bool v3)
    {
        if (chunkGraph == null)
        {
            return;
        }

        foreach (var node in chunkGraph.Graph.Values)
        {
            Gizmos.color = GetNodeColor(node, v3);
            Gizmos.DrawSphere(node.pos * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f), v1);

            // foreach (var edge in node.edges)
            // {
            //     Gizmos.color = v3 ? new Color(0.3f, 0.8f, 1f, 0.45f) : new Color(0.85f, 0.85f, 0.85f, 0.35f);
            //     Gizmos.DrawLine(edge.From.pos * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f), edge.To.pos * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f));
            // }
        }
    }

    private void DrawGraphForTiles(GraphPathfinder graph, float nodeRadius, float edgeWidth, bool isChunkGraph)
    {
        if (graph == null)
        {
            return;
        }

        foreach (var node in graph.Graph.Values)
        {
            Gizmos.color = GetNodeColor(node, isChunkGraph);
            Gizmos.DrawSphere(node.pos, nodeRadius);

            foreach (var edge in node.edges)
            {
                Gizmos.color = isChunkGraph ? new Color(0.3f, 0.8f, 1f, 0.45f) : new Color(0.85f, 0.85f, 0.85f, 0.35f);
                Gizmos.DrawLine(edge.From.pos, edge.To.pos);
            }
        }
    }

    private void DrawPath()
    {
        if (path == null || path.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawWireSphere(path[i], 0.18f);

            if (i < path.Count - 1)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }

    private void DrawGatewayPoints()
    {
        GraphPathfinder chunkGraph = hpaPathfinder.ChunkGraph;
        if (chunkGraph == null)
        {
            return;
        }

        var drawnPairs = new HashSet<string>();

        foreach (var posChunk in hpaPathfinder.TileGraphsByChunk.Keys)
        {
            var node = chunkGraph.GetNode(posChunk);
            if (node == null)
            {
                continue;
            }

            Vector2 fromCenter = GetChunkWorldCenter(node.pos);

            foreach (var edge in node.edges)
            {
                Vector2Int fromChunk = Vector2Int.RoundToInt(node.pos);
                Vector2Int toChunk = Vector2Int.RoundToInt(edge.To.pos);

                string pairKey = GetUndirectedEdgeKey(fromChunk, toChunk);
                if (!drawnPairs.Add(pairKey))
                {
                    continue;
                }

                Vector2 toCenter = GetChunkWorldCenter(edge.To.pos);
                Vector2 gatewayPoint = (fromCenter + toCenter) * 0.5f;

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(gatewayPoint, 0.22f);
                Gizmos.DrawLine(fromCenter, gatewayPoint);
                Gizmos.DrawLine(gatewayPoint, toCenter);
            }
        }
    }

    private static string GetUndirectedEdgeKey(Vector2Int a, Vector2Int b)
    {
        if (a.x > b.x || (a.x == b.x && a.y > b.y))
        {
            Vector2Int temp = a;
            a = b;
            b = temp;
        }

        return $"{a.x},{a.y}|{b.x},{b.y}";
    }

    private static Vector2 GetChunkWorldCenter(Vector2 chunkPosition)
    {
        return chunkPosition * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f);
    }

    private static Color GetNodeColor(NodeGraph node, bool isChunkGraph)
    {
        if (!node.walkable)
        {
            return Color.red;
        }

        if (node.debugPath)
        {
            return Color.green;
        }

        if (node.debugClosed)
        {
            return new Color(1f, 0.35f, 0.35f, 1f);
        }

        if (node.debugOpen)
        {
            return new Color(1f, 0.85f, 0.25f, 1f);
        }

        return isChunkGraph ? Color.cyan : Color.white;
    }
}
