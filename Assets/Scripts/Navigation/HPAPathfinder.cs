using System.Collections.Generic;
using UnityEngine;

public sealed class HPAPathfinder
{
    // Coarse graph used to plan the route across chunks.
    private GraphPathfinder chunkGraph;

    // Cache of per-chunk tile graphs so we only build detailed graphs when needed.
    private readonly Dictionary<Vector2Int, GraphPathfinder> tileGraphsByChunk = new Dictionary<Vector2Int, GraphPathfinder>();

    // Last successful or attempted search data for debug drawing.
    private List<Vector2> lastPath;
    private List<Vector2Int> lastChunkRoute;
    private List<Vector2> lastGatewayPoints;
    private List<Vector2> lastGatewayEntryPoints;
    private List<Vector2> lastGatewayExitPoints;

    // When true, a tile graph is removed after the chunk no longer has active creatures.
    public bool AutoReleaseUnusedChunkGraphs { get; set; } = true;

    // Controls how many gateway candidates are sampled along a shared chunk border.
    public int GatewayCountPerBorder { get; set; } = Mathf.Max(1, LocalTestValue.tilesPerChunk);

    // Controls how many tiles inward we search when the outermost gateway tile is blocked.
    public int GatewaySearchDepth { get; set; } = 2;

    // Expose the current chunk graph for debug rendering.
    public GraphPathfinder ChunkGraph => chunkGraph;

    // Expose cached tile graphs for debug rendering.
    public IReadOnlyDictionary<Vector2Int, GraphPathfinder> TileGraphsByChunk => tileGraphsByChunk;

    // Expose the most recent path and chunk route so debug tools can render them.
    public IReadOnlyList<Vector2> LastPath => lastPath;
    public IReadOnlyList<Vector2Int> LastChunkRoute => lastChunkRoute;
    public IReadOnlyList<Vector2> LastGatewayPoints => lastGatewayPoints;
    public IReadOnlyList<Vector2> LastGatewayEntryPoints => lastGatewayEntryPoints;
    public IReadOnlyList<Vector2> LastGatewayExitPoints => lastGatewayExitPoints;

    // Expose the requested start and goal positions for the last query.
    public Vector2 LastStartPosition { get; private set; }
    public Vector2 LastGoalPosition { get; private set; }

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

        // Reset the last-search cache before building a new result.
        lastPath = null;
        lastChunkRoute = null;
        lastGatewayPoints = null;
        lastGatewayEntryPoints = null;
        lastGatewayExitPoints = null;
        LastStartPosition = startPosition;
        LastGoalPosition = goalPosition;

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
        var selectedEntryPoints = new List<Vector2>();
        var selectedExitPoints = new List<Vector2>();

        for (int i = 0; i < chunkRoute.Count; i++)
        {
            Vector2Int currentChunk = chunkRoute[i];
            // Build or reuse the detailed tile graph for this chunk.
            GraphPathfinder tileGraph = GetOrCreateTileGraph(currentChunk);
            // Pick the best entry/exit gateway pair and then find the path inside the current chunk.
            Vector2Int? previousChunk = i > 0 ? chunkRoute[i - 1] : null;
            Vector2Int? nextChunk = i < chunkRoute.Count - 1 ? chunkRoute[i + 1] : null;

            if (!TryFindBestChunkSegment(tileGraph, currentChunk, previousChunk, nextChunk, startPosition, goalPosition, out List<Vector2> segment, out Vector2 selectedEntry, out Vector2 selectedExit))
            {
                return null;
            }

            selectedEntryPoints.Add(selectedEntry);
            selectedExitPoints.Add(selectedExit);

            // Merge all chunk segments into one continuous world path.
            AppendSegment(finalPath, segment);
        }

        // Cache the final result for debug rendering.
        lastPath = new List<Vector2>(finalPath);
        lastChunkRoute = new List<Vector2Int>(chunkRoute);
        lastGatewayPoints = BuildGatewayMidpoints(selectedEntryPoints, selectedExitPoints);
        lastGatewayEntryPoints = new List<Vector2>(selectedEntryPoints);
        lastGatewayExitPoints = new List<Vector2>(selectedExitPoints);

        if (AutoReleaseUnusedChunkGraphs)
        {
            ReleaseUnusedChunkGraphsExcept(lastChunkRoute);
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
        List<Vector2> rawChunkPath = FindPathOnGraph(chunkGraph, startNodePos, goalNodePos, true);

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
                chunkGraph.AddNodeByVector2(new Vector2(x, y), true, 1f);
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
            SyncTileGraphMoveCosts(existingGraph, chunkPosition);
            return existingGraph;
        }

        // Create a tile graph that lives entirely inside one chunk.
        var graph = new GraphPathfinder();
        Vector2 chunkOrigin = GetChunkOrigin(chunkPosition);
        Chunk worldChunk = GetChunk(chunkPosition);

        // Add one node per tile center inside the chunk.
        for (int x = 0; x < LocalTestValue.tilesPerChunk; x++)
        {
            for (int y = 0; y < LocalTestValue.tilesPerChunk; y++)
            {
                Vector2 nodePosition = new Vector2(chunkOrigin.x + x + 0.5f, chunkOrigin.y + y + 0.5f);
                graph.AddNodeByVector2(nodePosition, true, GetTileMoveCost(worldChunk, nodePosition));
            }
        }

        SyncTileGraphMoveCosts(graph, chunkPosition);

        // Connect neighboring tiles so the local search can move in 8 directions.
        foreach (var node in graph.Graph.Values)
        {
            node.FindEdges8Dir(graph);
        }

        // Store the result so later searches can reuse it.
        tileGraphsByChunk[chunkPosition] = graph;
        return graph;
    }

    private bool TryFindBestChunkSegment(
        GraphPathfinder tileGraph,
        Vector2Int chunkPosition,
        Vector2Int? previousChunk,
        Vector2Int? nextChunk,
        Vector2 startPosition,
        Vector2 goalPosition,
        out List<Vector2> bestSegment,
        out Vector2 selectedEntry,
        out Vector2 selectedExit)
    {
        bestSegment = null;
        selectedEntry = default;
        selectedExit = default;

        int maxDepth = Mathf.Clamp(GatewaySearchDepth, 0, LocalTestValue.tilesPerChunk - 1);

        // Prefer the outer border first. Only move inward if no walkable portal exists there.
        for (int depth = 0; depth <= maxDepth; depth++)
        {
            List<Vector2> entryCandidates = previousChunk.HasValue
                ? GetGatewayCandidatesAtDepth(chunkPosition, previousChunk.Value, depth)
                : new List<Vector2> { startPosition };

            List<Vector2> exitCandidates = nextChunk.HasValue
                ? GetGatewayCandidatesAtDepth(chunkPosition, nextChunk.Value, depth)
                : new List<Vector2> { goalPosition };

            if (TryFindBestSegmentFromCandidates(tileGraph, chunkPosition, entryCandidates, exitCandidates, out bestSegment, out selectedEntry, out selectedExit))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryFindBestSegmentFromCandidates(
        GraphPathfinder tileGraph,
        Vector2Int chunkPosition,
        List<Vector2> entryCandidates,
        List<Vector2> exitCandidates,
        out List<Vector2> bestSegment,
        out Vector2 selectedEntry,
        out Vector2 selectedExit)
    {
        bestSegment = null;
        selectedEntry = default;
        selectedExit = default;

        float bestCost = float.PositiveInfinity;
        bool foundAny = false;

        foreach (var entryCandidate in entryCandidates)
        {
            Vector2 snappedEntry = SnapToTileCenter(chunkPosition, entryCandidate);
            if (!IsWalkableNode(tileGraph, snappedEntry))
            {
                continue;
            }

            foreach (var exitCandidate in exitCandidates)
            {
                Vector2 snappedExit = SnapToTileCenter(chunkPosition, exitCandidate);
                if (!IsWalkableNode(tileGraph, snappedExit))
                {
                    continue;
                }

                List<Vector2> trialSegment = FindPathOnGraph(tileGraph, snappedEntry, snappedExit, false);
                if (trialSegment == null)
                {
                    continue;
                }

                float trialCost = CalculateSegmentCost(tileGraph, snappedEntry, trialSegment);
                if (trialCost < bestCost)
                {
                    bestCost = trialCost;
                    bestSegment = trialSegment;
                    selectedEntry = snappedEntry;
                    selectedExit = snappedExit;
                    foundAny = true;
                }
            }
        }

        if (!foundAny)
        {
            return false;
        }

        bestSegment = FindPathOnGraph(tileGraph, selectedEntry, selectedExit, true);
        return bestSegment != null;
    }

    private static List<Vector2> FindPathOnGraph(GraphPathfinder graph, Vector2 startPosition, Vector2 goalPosition, bool recordDebug)
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

        float heuristicScale = GetMinimumMoveCost(graph);

        // openSet = candidates to evaluate, closedSet = nodes already processed.
        var openSet = new List<NodeGraph> { startNode };
        var closedSet = new HashSet<NodeGraph>();

        // Seed the starting node.
        startNode.gCost = 0f;
        startNode.hCost = Vector2.Distance(startNode.pos, goalNode.pos) * heuristicScale;
        if (recordDebug)
        {
            startNode.debugOpen = true;
        }

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
            if (recordDebug)
            {
                currentNode.debugOpen = false;
                currentNode.debugClosed = true;
            }

            // Reached the goal, so rebuild the path by following parent links.
            if (currentNode == goalNode)
            {
                return RetracePath(startNode, goalNode, recordDebug);
            }

            // Evaluate every reachable neighbor.
            foreach (var edge in currentNode.edges)
            {
                NodeGraph neighbor = edge.To;

                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float newMovementCost = currentNode.gCost + (edge.cost * neighbor.moveCost);
                if (newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    // Store the best route found so far to this neighbor.
                    neighbor.gCost = newMovementCost;
                    neighbor.hCost = Vector2.Distance(neighbor.pos, goalNode.pos) * heuristicScale;
                    neighbor.parent = currentNode;
                    if (recordDebug)
                    {
                        neighbor.debugOpen = true;
                    }

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private static List<Vector2> RetracePath(NodeGraph startNode, NodeGraph goalNode, bool recordDebug)
    {
        // Walk from goal back to start using parent pointers.
        var path = new List<Vector2>();
        NodeGraph currentNode = goalNode;

        while (currentNode != startNode)
        {
            // Mark the final route for gizmo drawing.
            if (recordDebug)
            {
                currentNode.debugPath = true;
            }
            path.Add(currentNode.pos);
            currentNode = currentNode.parent;

            if (currentNode == null)
            {
                return null;
            }
        }

        // Mark the start node too, then reverse so the order is start -> goal.
        if (recordDebug)
        {
            startNode.debugPath = true;
        }
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

    private void ReleaseUnusedChunkGraphsExcept(IReadOnlyCollection<Vector2Int> protectedChunks)
    {
        var protectedChunkSet = protectedChunks != null ? new HashSet<Vector2Int>(protectedChunks) : new HashSet<Vector2Int>();
        var chunksToInspect = new List<Vector2Int>(tileGraphsByChunk.Keys);

        foreach (var chunkPosition in chunksToInspect)
        {
            if (protectedChunkSet.Contains(chunkPosition))
            {
                continue;
            }

            if (ThingHandler.GetCreatureCountInChunk(chunkPosition) == 0)
            {
                tileGraphsByChunk.Remove(chunkPosition);
            }
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

    private static List<Vector2> BuildGatewayMidpoints(IReadOnlyList<Vector2> entryPoints, IReadOnlyList<Vector2> exitPoints)
    {
        var gateways = new List<Vector2>();

        if (entryPoints == null || exitPoints == null)
        {
            return gateways;
        }

        int boundaryCount = Mathf.Min(entryPoints.Count, exitPoints.Count) - 1;
        for (int i = 0; i < boundaryCount; i++)
        {
            gateways.Add((exitPoints[i] + entryPoints[i + 1]) * 0.5f);
        }

        return gateways;
    }

    private List<Vector2> GetGatewayCandidatesAtDepth(Vector2Int fromChunk, Vector2Int toChunk, int depth)
    {
        var candidates = new List<Vector2>();
        Vector2Int delta = toChunk - fromChunk;

        if (delta == Vector2Int.zero)
        {
            return candidates;
        }

        if (delta.x != 0 && delta.y != 0)
        {
            AddDiagonalGatewayCandidates(candidates, fromChunk, delta, depth);
        }
        else
        {
            AddAxisAlignedGatewayCandidates(candidates, fromChunk, delta, depth);
        }

        return candidates;
    }

    private void AddAxisAlignedGatewayCandidates(List<Vector2> candidates, Vector2Int fromChunk, Vector2Int delta, int depth)
    {
        var sampleIndices = BuildSampleTileIndices();

        if (delta.x != 0)
        {
            int edgeXIndex = delta.x > 0 ? LocalTestValue.tilesPerChunk - 1 : 0;

            foreach (int yIndex in sampleIndices)
            {
                int xIndex = delta.x > 0 ? edgeXIndex - depth : edgeXIndex + depth;
                if (!IsValidTileIndex(xIndex, yIndex))
                {
                    continue;
                }

                AddUniqueCandidate(candidates, GetTileCenter(fromChunk, xIndex, yIndex));
            }
        }
        else
        {
            int edgeYIndex = delta.y > 0 ? LocalTestValue.tilesPerChunk - 1 : 0;

            foreach (int xIndex in sampleIndices)
            {
                int yIndex = delta.y > 0 ? edgeYIndex - depth : edgeYIndex + depth;
                if (!IsValidTileIndex(xIndex, yIndex))
                {
                    continue;
                }

                AddUniqueCandidate(candidates, GetTileCenter(fromChunk, xIndex, yIndex));
            }
        }
    }

    private void AddDiagonalGatewayCandidates(List<Vector2> candidates, Vector2Int fromChunk, Vector2Int delta, int depth)
    {
        int edgeXIndex = delta.x > 0 ? LocalTestValue.tilesPerChunk - 1 : 0;
        int edgeYIndex = delta.y > 0 ? LocalTestValue.tilesPerChunk - 1 : 0;
        int inwardXStep = delta.x > 0 ? -1 : 1;
        int inwardYStep = delta.y > 0 ? -1 : 1;

        int cornerX = edgeXIndex + inwardXStep * depth;
        int cornerY = edgeYIndex + inwardYStep * depth;

        if (IsValidTileIndex(cornerX, edgeYIndex))
        {
            AddUniqueCandidate(candidates, GetTileCenter(fromChunk, cornerX, edgeYIndex));
        }

        if (IsValidTileIndex(edgeXIndex, cornerY))
        {
            AddUniqueCandidate(candidates, GetTileCenter(fromChunk, edgeXIndex, cornerY));
        }

        if (IsValidTileIndex(cornerX, cornerY))
        {
            AddUniqueCandidate(candidates, GetTileCenter(fromChunk, cornerX, cornerY));
        }
    }

    private List<int> BuildSampleTileIndices()
    {
        int sampleCount = Mathf.Clamp(GatewayCountPerBorder, 1, LocalTestValue.tilesPerChunk);
        var indices = new List<int>(sampleCount);
        var seen = new HashSet<int>();

        if (sampleCount == 1)
        {
            int centerIndex = Mathf.RoundToInt((LocalTestValue.tilesPerChunk - 1) * 0.5f);
            indices.Add(centerIndex);
            return indices;
        }

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            int index = Mathf.RoundToInt(t * (LocalTestValue.tilesPerChunk - 1));
            if (seen.Add(index))
            {
                indices.Add(index);
            }
        }

        return indices;
    }

    private static bool IsValidTileIndex(int xIndex, int yIndex)
    {
        return xIndex >= 0 && xIndex < LocalTestValue.tilesPerChunk &&
               yIndex >= 0 && yIndex < LocalTestValue.tilesPerChunk;
    }

    private static Vector2 GetTileCenter(Vector2Int chunkPosition, int tileX, int tileY)
    {
        Vector2 origin = GetChunkOrigin(chunkPosition);
        return new Vector2(origin.x + tileX + 0.5f, origin.y + tileY + 0.5f);
    }

    private static void AddUniqueCandidate(List<Vector2> candidates, Vector2 candidate)
    {
        if (!candidates.Contains(candidate))
        {
            candidates.Add(candidate);
        }
    }

    private static bool IsWalkableNode(GraphPathfinder graph, Vector2 position)
    {
        NodeGraph node = graph.GetNode(position);
        return node != null && node.walkable;
    }

    private static float CalculateSegmentCost(GraphPathfinder graph, Vector2 startPosition, List<Vector2> segment)
    {
        float totalCost = 0f;
        Vector2 previousPoint = startPosition;

        for (int i = 0; i < segment.Count; i++)
        {
            Vector2 currentPoint = segment[i];
            NodeGraph node = graph.GetNode(currentPoint);
            float moveCost = node != null ? node.moveCost : 1f;
            totalCost += Vector2.Distance(previousPoint, currentPoint) * moveCost;
            previousPoint = currentPoint;
        }
        return totalCost;
    }

    private Chunk GetChunk(Vector2Int chunkPosition)
    {
        GameService gameService = GameService.Ins;
        if (gameService == null || gameService.WorldHandler == null)
        {
            return null;
        }

        Vector2 worldPosition = new Vector2(
            chunkPosition.x * LocalTestValue.tilesPerChunk,
            chunkPosition.y * LocalTestValue.tilesPerChunk);
        return gameService.WorldHandler.GetChunk(worldPosition);
    }

    private static float GetTileMoveCost(Chunk chunk, Vector2 worldPosition)
    {
        if (chunk == null)
        {
            return 1f;
        }

        Tile tile = chunk.GetTileAtWorldPosition(worldPosition);
        if (tile == null)
        {
            return 1f;
        }

        return Mathf.Max(0f, tile.moveCost);
    }

    private void SyncTileGraphMoveCosts(GraphPathfinder graph, Vector2Int chunkPosition)
    {
        Chunk worldChunk = GetChunk(chunkPosition);
        if (worldChunk == null)
        {
            return;
        }

        foreach (var node in graph.Graph.Values)
        {
            node.moveCost = GetTileMoveCost(worldChunk, node.pos);
        }
    }

    private static float GetMinimumMoveCost(GraphPathfinder graph)
    {
        float minimumMoveCost = float.PositiveInfinity;

        foreach (var node in graph.Graph.Values)
        {
            if (!node.walkable)
            {
                continue;
            }

            if (node.moveCost < minimumMoveCost)
            {
                minimumMoveCost = node.moveCost;
            }
        }

        if (float.IsPositiveInfinity(minimumMoveCost))
        {
            return 1f;
        }

        return Mathf.Max(0f, minimumMoveCost);
    }

    private static Vector2 GetChunkOrigin(Vector2Int chunkPosition)
    {
        // Convert a chunk coordinate to the world-space origin of that chunk.
        return new Vector2(chunkPosition.x * LocalTestValue.tilesPerChunk, chunkPosition.y * LocalTestValue.tilesPerChunk);
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