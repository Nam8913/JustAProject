using System.Collections.Generic;
using UnityEngine;

public sealed class HPAPathfinder
{
    // Coarse graph used to plan the route across chunks.
    private GraphPathfinder chunkGraph;

    // Cache of per-chunk tile graphs so we only build detailed graphs when needed.
    private readonly Dictionary<Vector2Int, GraphPathfinder> tileGraphsByChunk = new Dictionary<Vector2Int, GraphPathfinder>();

    // When true, a tile graph is removed after the chunk no longer has active creatures.
    public bool AutoReleaseUnusedChunkGraphs { get; set; } = true;

    // Expose the current chunk graph for debug rendering.
    public GraphPathfinder ChunkGraph => chunkGraph;

    // Expose cached tile graphs for debug rendering.
    public IReadOnlyDictionary<Vector2Int, GraphPathfinder> TileGraphsByChunk => tileGraphsByChunk;

    public HPAPathfinder()
    {
        // Start with an empty coarse graph; it will be rebuilt per path query.
        chunkGraph = new GraphPathfinder();
    }

    public void PrewarmChunkGraphAround(Vector2 playerPosition)
    {
        // Build the chunk-level graph around the player so the first query has a ready graph.
        Vector2Int playerChunk = Chunk.GetChunkPosition(playerPosition);
        BuildChunkGraph(playerChunk, playerChunk);
    }

    public List<Vector2> GetPathFromTo(Vector2 startPosition, Vector2 goalPosition)
    {
        // Clear old gizmo flags so a new search does not leave stale path colors behind.
        ClearDebugVisuals();

        // Convert world positions to chunk coordinates first.
        Vector2Int startChunk = Chunk.GetChunkPosition(startPosition);
        Vector2Int goalChunk = Chunk.GetChunkPosition(goalPosition);

        // Step 1: find a path across chunks.
        List<Vector2Int> chunkRoute = FindChunkRoute(startChunk, goalChunk);
        if (chunkRoute == null || chunkRoute.Count == 0)
        {
            return null;
        }

        // Step 2: refine each chunk in the route into tile-level movement.
        var finalPath = new List<Vector2>();

        for (int i = 0; i < chunkRoute.Count; i++)
        {
            Vector2Int currentChunk = chunkRoute[i];
            // For the first chunk, start from the exact world position.
            // For the next chunks, start from the gateway toward the previous chunk.
            Vector2 entryPoint = i == 0 ? startPosition : GetGatewayPoint(currentChunk, chunkRoute[i - 1]);
            // For the last chunk, end at the exact world goal.
            // For middle chunks, end at the gateway toward the next chunk.
            Vector2 exitPoint = i == chunkRoute.Count - 1 ? goalPosition : GetGatewayPoint(currentChunk, chunkRoute[i + 1]);

            // Build or reuse the detailed tile graph for this chunk.
            GraphPathfinder tileGraph = GetOrCreateTileGraph(currentChunk);
            // Find the path inside the current chunk only.
            List<Vector2> segment = FindPathOnGraph(tileGraph, SnapToTileCenter(currentChunk, entryPoint), SnapToTileCenter(currentChunk, exitPoint));

            if (segment == null)
            {
                return null;
            }

            // Merge all chunk segments into one continuous world path.
            AppendSegment(finalPath, segment);

            // Optional memory cleanup: remove the detailed graph if nobody is using this chunk.
            if (AutoReleaseUnusedChunkGraphs && ThingHandler.GetCreatureCountInChunk(currentChunk) == 0)
            {
                tileGraphsByChunk.Remove(currentChunk);
            }
        }

        return finalPath;
    }

    public void ReleaseChunkGraphIfUnused(Vector2Int chunkPosition)
    {
        // Manual cleanup entry point for gameplay code.
        if (ThingHandler.GetCreatureCountInChunk(chunkPosition) == 0)
        {
            tileGraphsByChunk.Remove(chunkPosition);
        }
    }

    private List<Vector2Int> FindChunkRoute(Vector2Int startChunk, Vector2Int goalChunk)
    {
        // Rebuild the abstract graph so it covers the current start/goal range.
        BuildChunkGraph(startChunk, goalChunk);

        // Run A* on chunk coordinates, then convert the result into chunk positions.
        Vector2 startNodePos = new Vector2(startChunk.x, startChunk.y);
        Vector2 goalNodePos = new Vector2(goalChunk.x, goalChunk.y);
        List<Vector2> rawChunkPath = FindPathOnGraph(chunkGraph, startNodePos, goalNodePos);

        if (rawChunkPath == null)
        {
            return null;
        }

        var chunkRoute = new List<Vector2Int> { startChunk };
        foreach (var chunkPosition in rawChunkPath)
        {
            chunkRoute.Add(new Vector2Int(Mathf.RoundToInt(chunkPosition.x), Mathf.RoundToInt(chunkPosition.y)));
        }

        return chunkRoute;
    }

    private void BuildChunkGraph(Vector2Int startChunk, Vector2Int goalChunk)
    {
        // Add a small margin so the graph has neighbors around the endpoints.
        int margin = Mathf.Max(1, LocalTestValue.maxChunkWorkAroundPlayer);
        int minX = Mathf.Min(startChunk.x, goalChunk.x) - margin;
        int maxX = Mathf.Max(startChunk.x, goalChunk.x) + margin;
        int minY = Mathf.Min(startChunk.y, goalChunk.y) - margin;
        int maxY = Mathf.Max(startChunk.y, goalChunk.y) + margin;

        // Start fresh for this query.
        chunkGraph = new GraphPathfinder();

        // Create one node per chunk coordinate in the query area.
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                chunkGraph.AddNodeByVector2(new Vector2(x, y), true);
            }
        }

        // Connect chunk nodes to all 8 neighboring chunks.
        foreach (var node in chunkGraph.Graph.Values)
        {
            node.FindEdges8Dir(chunkGraph);
        }
    }

    private GraphPathfinder GetOrCreateTileGraph(Vector2Int chunkPosition)
    {
        // Reuse the cached tile graph if the chunk was already built.
        if (tileGraphsByChunk.TryGetValue(chunkPosition, out GraphPathfinder existingGraph))
        {
            return existingGraph;
        }

        // Create a tile graph that lives entirely inside one chunk.
        var graph = new GraphPathfinder();
        Vector2 chunkOrigin = GetChunkOrigin(chunkPosition);

        // Add one node per tile center inside the chunk.
        for (int x = 0; x < LocalTestValue.tilesPerChunk; x++)
        {
            for (int y = 0; y < LocalTestValue.tilesPerChunk; y++)
            {
                Vector2 nodePosition = new Vector2(chunkOrigin.x + x + 0.5f, chunkOrigin.y + y + 0.5f);
                graph.AddNodeByVector2(nodePosition, true);
            }
        }

        // Connect neighboring tiles so the local search can move in 8 directions.
        foreach (var node in graph.Graph.Values)
        {
            node.FindEdges8Dir(graph);
        }

        // Store the result so later searches can reuse it.
        tileGraphsByChunk[chunkPosition] = graph;
        return graph;
    }

    private static List<Vector2> FindPathOnGraph(GraphPathfinder graph, Vector2 startPosition, Vector2 goalPosition)
    {
        // Look up the node nearest to the requested start/end positions.
        NodeGraph startNode = graph.GetNode(startPosition);
        NodeGraph goalNode = graph.GetNode(goalPosition);

        if (startNode == null || goalNode == null)
        {
            return null;
        }

        // Reset previous search state before running A* again.
        ResetNodes(graph);

        // openSet = candidates to evaluate, closedSet = nodes already processed.
        var openSet = new List<NodeGraph> { startNode };
        var closedSet = new HashSet<NodeGraph>();

        // Seed the starting node.
        startNode.gCost = 0f;
        startNode.hCost = Vector2.Distance(startNode.pos, goalNode.pos);
        startNode.debugOpen = true;

        while (openSet.Count > 0)
        {
            // Pick the node with the lowest total cost.
            NodeGraph currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                NodeGraph candidate = openSet[i];
                if (candidate.fCost < currentNode.fCost || (candidate.fCost == currentNode.fCost && candidate.hCost < currentNode.hCost))
                {
                    currentNode = candidate;
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            currentNode.debugOpen = false;
            currentNode.debugClosed = true;

            // Reached the goal, so rebuild the path by following parent links.
            if (currentNode == goalNode)
            {
                return RetracePath(startNode, goalNode);
            }

            // Evaluate every reachable neighbor.
            foreach (var edge in currentNode.edges)
            {
                NodeGraph neighbor = edge.To;

                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCost = currentNode.gCost + edge.cost;
                if (newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    // Store the best route found so far to this neighbor.
                    neighbor.gCost = newMovementCost;
                    neighbor.hCost = Vector2.Distance(neighbor.pos, goalNode.pos);
                    neighbor.parent = currentNode;
                    neighbor.debugOpen = true;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private static List<Vector2> RetracePath(NodeGraph startNode, NodeGraph goalNode)
    {
        // Walk from goal back to start using parent pointers.
        var path = new List<Vector2>();
        NodeGraph currentNode = goalNode;

        while (currentNode != startNode)
        {
            // Mark the final route for gizmo drawing.
            currentNode.debugPath = true;
            path.Add(currentNode.pos);
            currentNode = currentNode.parent;

            if (currentNode == null)
            {
                return null;
            }
        }

        // Mark the start node too, then reverse so the order is start -> goal.
        startNode.debugPath = true;
        path.Reverse();
        return path;
    }

    private static void ResetNodes(GraphPathfinder graph)
    {
        // Every node starts in a neutral state before a new search.
        foreach (var node in graph.Graph.Values)
        {
            node.gCost = float.PositiveInfinity;
            node.hCost = 0f;
            node.parent = null;
            node.debugOpen = false;
            node.debugClosed = false;
            node.debugPath = false;
        }
    }

    private void ClearDebugVisuals()
    {
        // Clear debug colors in the abstract graph.
        ClearDebugVisuals(chunkGraph);

        // Clear debug colors in every cached tile graph.
        foreach (var graph in tileGraphsByChunk.Values)
        {
            ClearDebugVisuals(graph);
        }
    }

    private static void ClearDebugVisuals(GraphPathfinder graph)
    {
        if (graph == null)
        {
            return;
        }

        // Leave only the topology, remove all temporary search markings.
        foreach (var node in graph.Graph.Values)
        {
            node.debugOpen = false;
            node.debugClosed = false;
            node.debugPath = false;
        }
    }

    private static void AppendSegment(List<Vector2> finalPath, List<Vector2> segment)
    {
        // Merge chunk segments while skipping the duplicate point at the boundary.
        for (int i = 0; i < segment.Count; i++)
        {
            Vector2 step = segment[i];
            if (finalPath.Count > 0 && finalPath[finalPath.Count - 1] == step)
            {
                continue;
            }

            finalPath.Add(step);
        }
    }

    private static Vector2 GetChunkOrigin(Vector2Int chunkPosition)
    {
        // Convert a chunk coordinate to the world-space origin of that chunk.
        return new Vector2(chunkPosition.x * LocalTestValue.tilesPerChunk, chunkPosition.y * LocalTestValue.tilesPerChunk);
    }

    private static Vector2 GetGatewayPoint(Vector2Int fromChunk, Vector2Int toChunk)
    {
        // This is a simple placeholder gateway: the border point between two neighboring chunks.
        Vector2 origin = GetChunkOrigin(fromChunk);
        int dx = Mathf.Clamp(toChunk.x - fromChunk.x, -1, 1);
        int dy = Mathf.Clamp(toChunk.y - fromChunk.y, -1, 1);

        float center = (LocalTestValue.tilesPerChunk - 1) * 0.5f;
        float x = origin.x + center;
        float y = origin.y + center;

        if (dx > 0)
        {
            x = origin.x + LocalTestValue.tilesPerChunk - 0.5f;
        }
        else if (dx < 0)
        {
            x = origin.x + 0.5f;
        }

        if (dy > 0)
        {
            y = origin.y + LocalTestValue.tilesPerChunk - 0.5f;
        }
        else if (dy < 0)
        {
            y = origin.y + 0.5f;
        }

        return new Vector2(x, y);
    }

    private static Vector2 SnapToTileCenter(Vector2Int chunkPosition, Vector2 position)
    {
        // Force a world position onto a valid tile center inside the chunk.
        Vector2 origin = GetChunkOrigin(chunkPosition);
        float localX = Mathf.Clamp(Mathf.Round(position.x - origin.x - 0.5f), 0, LocalTestValue.tilesPerChunk - 1);
        float localY = Mathf.Clamp(Mathf.Round(position.y - origin.y - 0.5f), 0, LocalTestValue.tilesPerChunk - 1);

        return new Vector2(origin.x + localX + 0.5f, origin.y + localY + 0.5f);
    }
}