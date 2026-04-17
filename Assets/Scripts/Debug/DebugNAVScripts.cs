using System.Collections.Generic;
using UnityEngine;

public class DebugNAVScripts : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private bool isDebugMode = true;
    [SerializeField] private bool autoBindSceneReferences = true;
    [SerializeField] private bool showStatsOverlay = true;

    [Header("World")]
    [SerializeField] private bool showActiveChunks = true;
    [SerializeField] private bool showCreatures = true;

    [Header("Pathfinding")]
    [SerializeField] private bool showChunkMemoryInPathfinding = true;
    [SerializeField] private bool showTilesInChunkMemoryInPathfinding = true;
    [SerializeField] private bool showOnlyLastRouteChunks = false;
    [SerializeField] private bool showFocusedPortalOnly = true;
    [SerializeField] private int focusedPortalBoundaryIndex = 0;
    [SerializeField] private bool showChunkGraphEdges = false;
    [SerializeField] private bool showTileGraphEdges = false;
    [SerializeField] private bool showGatewayPoints = true;
    [SerializeField] private bool showLastPath = true;
    [SerializeField] private bool showPathEndpoints = true;

    [Header("Sizes")]
    [SerializeField] private float chunkNodeRadius = 0.18f;
    [SerializeField] private float tileNodeRadius = 0.10f;
    [SerializeField] private float creatureRadius = 0.20f;
    [SerializeField] private float pathNodeRadius = 0.15f;
    [SerializeField] private float gatewayRadius = 0.18f;

    [Header("Colors")]
    [SerializeField] private Color activeChunkColor = Color.green;
    [SerializeField] private Color creatureColor = Color.magenta;
    [SerializeField] private Color chunkMemoryColor = Color.red;
    [SerializeField] private Color tileNodeColor = Color.blue;
    [SerializeField] private Color chunkEdgeColor = new Color(0.3f, 0.8f, 1f, 0.45f);
    [SerializeField] private Color tileEdgeColor = new Color(0.85f, 0.85f, 0.85f, 0.35f);
    [SerializeField] private Color gatewayColor = Color.yellow;
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private Color startColor = Color.cyan;
    [SerializeField] private Color goalColor = Color.red;

    private WorldHandler cachedWorldHandler;
    private NavService cachedNavService;

    private void OnEnable()
    {
        RefreshReferences();
    }

    private void OnValidate()
    {
        if (autoBindSceneReferences)
        {
            RefreshReferences();
        }
    }

    private void OnDrawGizmos()
    {
        if(!isDebugMode)
        {
            return;
        }

        RefreshReferencesIfNeeded();

        if(showActiveChunks)
        {
            ShowActiveChunks();
        }

        if(showCreatures)
        {
            ShowCreatures();
        }

        if(showChunkMemoryInPathfinding || showTilesInChunkMemoryInPathfinding || showGatewayPoints || showLastPath)
        {
            ShowPathfindingMemory();
        }
    }

    private void OnGUI()
    {
        if (!isDebugMode || !showStatsOverlay)
        {
            return;
        }

        RefreshReferencesIfNeeded();

        HPAPathfinder pathfinder = GetPathfinder();
        int activeChunkCount = cachedWorldHandler != null ? cachedWorldHandler.GetActiveChunkPositions().Count : 0;
        int creatureCount = CountCreatures();
        int cachedChunkGraphCount = pathfinder != null ? pathfinder.TileGraphsByChunk.Count : 0;
        int cachedTileNodeCount = CountTileNodes(pathfinder);
        int chunkGraphNodeCount = pathfinder != null && pathfinder.ChunkGraph != null ? pathfinder.ChunkGraph.Graph.Count : 0;
        int lastPathCount = pathfinder != null && pathfinder.LastPath != null ? pathfinder.LastPath.Count : 0;
        int lastChunkRouteCount = pathfinder != null && pathfinder.LastChunkRoute != null ? pathfinder.LastChunkRoute.Count : 0;
        int lastGatewayCount = pathfinder != null && pathfinder.LastGatewayEntryPoints != null ? pathfinder.LastGatewayEntryPoints.Count : 0;

        GUILayout.BeginArea(new Rect(10f, 10f, 420f, 240f), GUI.skin.box);
        GUILayout.Label("Pathfinding Debug");
        GUILayout.Label($"Active chunks: {activeChunkCount}");
        GUILayout.Label($"Creatures: {creatureCount}");
        GUILayout.Label($"Cached tile graphs: {cachedChunkGraphCount}");
        GUILayout.Label($"Chunk graph nodes: {chunkGraphNodeCount}");
        GUILayout.Label($"Cached tile nodes: {cachedTileNodeCount}");
        GUILayout.Label($"Last chunk route points: {lastChunkRouteCount}");
        GUILayout.Label($"Last gateway links: {lastGatewayCount}");
        GUILayout.Label($"Last path points: {lastPathCount}");
        GUILayout.Label($"Focus portal only: {showFocusedPortalOnly}");
        GUILayout.Label($"Focused portal boundary index: {focusedPortalBoundaryIndex}");

        if (pathfinder != null)
        {
            GUILayout.Label($"Last start: {pathfinder.LastStartPosition}");
            GUILayout.Label($"Last goal: {pathfinder.LastGoalPosition}");
            GUILayout.Label($"Gateway count per border: {pathfinder.GatewayCountPerBorder}");
            GUILayout.Label($"Gateway search depth: {pathfinder.GatewaySearchDepth}");
            GUILayout.Label($"Auto release unused graphs: {pathfinder.AutoReleaseUnusedChunkGraphs}");

            if (showFocusedPortalOnly && TryGetFocusedPortalInfo(pathfinder, out int boundaryIndex, out Vector2Int fromChunk, out Vector2Int toChunk, out Vector2 exitPoint, out Vector2 entryPoint))
            {
                GUILayout.Label($"Focused chunk pair: {fromChunk} -> {toChunk}");
                GUILayout.Label($"Focused boundary index: {boundaryIndex}");
                GUILayout.Label($"Portal exit: {exitPoint}");
                GUILayout.Label($"Portal entry: {entryPoint}");
            }
        }

        GUILayout.EndArea();
    }

    private void ShowCreatures()
    {
        if(ThingHandler.Things.Count <= 0)
        {
            return;
        }

        foreach(var thing in ThingHandler.Things)
        {
            if(thing is Creature creature)
            {
                Gizmos.color = creatureColor;
                Gizmos.DrawSphere(creature.transform.position, creatureRadius);
            }
        }
    }

    private void ShowActiveChunks()
    {
        if (cachedWorldHandler == null)
        {
            return;
        }

        foreach (var chunkPos in cachedWorldHandler.GetActiveChunkPositions())
        {
            Gizmos.color = activeChunkColor;
            DrawChunkWireCube(chunkPos, LocalTestValue.tilesPerChunk);
        }
    }

    private void ShowPathfindingMemory()
    {
        HPAPathfinder pathfinder = GetPathfinder();
        if (pathfinder == null)
        {
            return;
        }

        if (showFocusedPortalOnly)
        {
            if (showChunkMemoryInPathfinding)
            {
                ShowChunkGraph(pathfinder.ChunkGraph);
            }

            if (showTilesInChunkMemoryInPathfinding)
            {
                ShowTileGraphs(pathfinder.TileGraphsByChunk);
            }

            if (showGatewayPoints)
            {
                ShowFocusedPortal(pathfinder);
            }

            return;
        }

        if (showChunkMemoryInPathfinding)
        {
            ShowChunkGraph(pathfinder.ChunkGraph);
        }

        if (showTilesInChunkMemoryInPathfinding)
        {
            ShowTileGraphs(pathfinder.TileGraphsByChunk);
        }

        if (showGatewayPoints)
        {
            ShowGatewayPoints(pathfinder);
        }

        if (showLastPath)
        {
            ShowLastPath(pathfinder);
        }
    }

    private void ShowChunkGraph(GraphPathfinder chunkGraph)
    {
        if (chunkGraph == null)
        {
            return;
        }

        HPAPathfinder pathfinder = GetPathfinder();

        foreach (var node in chunkGraph.Graph.Values)
        {
            Vector2Int chunkPos = Vector2Int.RoundToInt(node.pos);
            if ((showOnlyLastRouteChunks || showFocusedPortalOnly) && !ShouldShowChunk(chunkPos, pathfinder))
            {
                continue;
            }

            Vector2 center = GetChunkCenter(chunkPos);

            Gizmos.color = GetDebugNodeColor(node, chunkMemoryColor);
            Gizmos.DrawWireCube(center, new Vector2(LocalTestValue.tilesPerChunk, LocalTestValue.tilesPerChunk));

            if (showChunkGraphEdges)
            {
                foreach (var edge in node.edges)
                {
                    if (!ShouldDrawEdgeOnce(node.pos, edge.To.pos))
                    {
                        continue;
                    }

                    Gizmos.color = chunkEdgeColor;
                    Gizmos.DrawLine(center, GetChunkCenter(Vector2Int.RoundToInt(edge.To.pos)));
                }
            }
        }
    }

    private void ShowTileGraphs(IReadOnlyDictionary<Vector2Int, GraphPathfinder> tileGraphsByChunk)
    {
        HPAPathfinder pathfinder = GetPathfinder();

        foreach (var chunkGraphPair in tileGraphsByChunk)
        {
            if ((showOnlyLastRouteChunks || showFocusedPortalOnly) && !ShouldShowChunk(chunkGraphPair.Key, pathfinder))
            {
                continue;
            }

            var tileGraph = chunkGraphPair.Value;
            if (tileGraph == null)
            {
                continue;
            }

            foreach (var node in tileGraph.Graph.Values)
            {
                Gizmos.color = GetDebugNodeColor(node, tileNodeColor);
                Gizmos.DrawSphere(node.pos, tileNodeRadius);

                if (!showTileGraphEdges)
                {
                    continue;
                }

                foreach (var edge in node.edges)
                {
                    if (!ShouldDrawEdgeOnce(node.pos, edge.To.pos))
                    {
                        continue;
                    }

                    Gizmos.color = tileEdgeColor;
                    Gizmos.DrawLine(edge.From.pos, edge.To.pos);
                }
            }
        }
    }

    private void ShowGatewayPoints(HPAPathfinder pathfinder)
    {
        if (showFocusedPortalOnly)
        {
            ShowFocusedPortal(pathfinder);
            return;
        }

        if (pathfinder.LastGatewayEntryPoints == null || pathfinder.LastGatewayExitPoints == null)
        {
            return;
        }

        int count = Mathf.Min(pathfinder.LastGatewayEntryPoints.Count, pathfinder.LastGatewayExitPoints.Count);
        for (int i = 0; i < count; i++)
        {
            Vector2 entryPoint = pathfinder.LastGatewayEntryPoints[i];
            Vector2 exitPoint = pathfinder.LastGatewayExitPoints[i];

            Gizmos.color = gatewayColor;
            Gizmos.DrawLine(entryPoint, exitPoint);

            Gizmos.color = startColor;
            Gizmos.DrawWireSphere(entryPoint, gatewayRadius);

            Gizmos.color = goalColor;
            Gizmos.DrawWireSphere(exitPoint, gatewayRadius);
        }
    }

    private void ShowFocusedPortal(HPAPathfinder pathfinder)
    {
        if (!TryGetFocusedPortalInfo(pathfinder, out int boundaryIndex, out Vector2Int fromChunk, out Vector2Int toChunk, out Vector2 exitPoint, out Vector2 entryPoint))
        {
            return;
        }

        Gizmos.color = gatewayColor;
        Gizmos.DrawLine(exitPoint, entryPoint);

        Gizmos.color = startColor;
        Gizmos.DrawWireSphere(exitPoint, gatewayRadius);

        Gizmos.color = goalColor;
        Gizmos.DrawWireSphere(entryPoint, gatewayRadius);
    }

    private void ShowLastPath(HPAPathfinder pathfinder)
    {
        if (pathfinder.LastPath == null || pathfinder.LastPath.Count == 0)
        {
            return;
        }

        if (showPathEndpoints)
        {
            Gizmos.color = startColor;
            Gizmos.DrawSphere(pathfinder.LastStartPosition, pathNodeRadius * 1.2f);

            Gizmos.color = goalColor;
            Gizmos.DrawSphere(pathfinder.LastGoalPosition, pathNodeRadius * 1.2f);
        }

        Gizmos.color = pathColor;
        Vector2 previousPoint = pathfinder.LastStartPosition;

        foreach (var point in pathfinder.LastPath)
        {
            Gizmos.DrawWireSphere(point, pathNodeRadius);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        Gizmos.DrawLine(previousPoint, pathfinder.LastGoalPosition);
    }

    private void RefreshReferences()
    {
        cachedWorldHandler = FindAnyObjectByType<WorldHandler>();
        cachedNavService = NavService.Instance;
    }

    private void RefreshReferencesIfNeeded()
    {
        if (!autoBindSceneReferences)
        {
            return;
        }

        if (cachedWorldHandler == null)
        {
            cachedWorldHandler = FindAnyObjectByType<WorldHandler>();
        }

        if (cachedNavService == null)
        {
            cachedNavService = NavService.Instance;
        }
    }

    private HPAPathfinder GetPathfinder()
    {
        return cachedNavService != null ? cachedNavService.Pathfinder : null;
    }

    private static int CountTileNodes(HPAPathfinder pathfinder)
    {
        if (pathfinder == null)
        {
            return 0;
        }

        int count = 0;
        foreach (var graphPair in pathfinder.TileGraphsByChunk)
        {
            if (graphPair.Value != null)
            {
                count += graphPair.Value.Graph.Count;
            }
        }

        return count;
    }

    private bool ShouldShowChunk(Vector2Int chunkPosition, HPAPathfinder pathfinder)
    {
        if (pathfinder == null || pathfinder.LastChunkRoute == null || pathfinder.LastChunkRoute.Count == 0)
        {
            return true;
        }

        if (FindFocusedPortalChunks(pathfinder, out Vector2Int focusedFromChunk, out Vector2Int focusedToChunk))
        {
            return chunkPosition == focusedFromChunk || chunkPosition == focusedToChunk;
        }

        for (int i = 0; i < pathfinder.LastChunkRoute.Count; i++)
        {
            if (pathfinder.LastChunkRoute[i] == chunkPosition)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetFocusedPortalInfo(HPAPathfinder pathfinder, out int boundaryIndex, out Vector2Int fromChunk, out Vector2Int toChunk, out Vector2 exitPoint, out Vector2 entryPoint)
    {
        boundaryIndex = 0;
        fromChunk = default;
        toChunk = default;
        exitPoint = default;
        entryPoint = default;

        if (!FindFocusedPortalChunks(pathfinder, out fromChunk, out toChunk, out boundaryIndex))
        {
            return false;
        }

        if (pathfinder == null || pathfinder.LastGatewayExitPoints == null || pathfinder.LastGatewayEntryPoints == null)
        {
            return false;
        }

        if (boundaryIndex < 0 || boundaryIndex >= pathfinder.LastGatewayExitPoints.Count || boundaryIndex + 1 >= pathfinder.LastGatewayEntryPoints.Count)
        {
            return false;
        }

        exitPoint = pathfinder.LastGatewayExitPoints[boundaryIndex];
        entryPoint = pathfinder.LastGatewayEntryPoints[boundaryIndex + 1];
        return true;
    }

    private bool FindFocusedPortalChunks(HPAPathfinder pathfinder, out Vector2Int fromChunk, out Vector2Int toChunk)
    {
        int ignoredBoundaryIndex;
        return FindFocusedPortalChunks(pathfinder, out fromChunk, out toChunk, out ignoredBoundaryIndex);
    }

    private bool FindFocusedPortalChunks(HPAPathfinder pathfinder, out Vector2Int fromChunk, out Vector2Int toChunk, out int boundaryIndex)
    {
        fromChunk = default;
        toChunk = default;
        boundaryIndex = 0;

        if (pathfinder == null || pathfinder.LastChunkRoute == null || pathfinder.LastChunkRoute.Count < 2)
        {
            return false;
        }

        boundaryIndex = Mathf.Clamp(focusedPortalBoundaryIndex, 0, pathfinder.LastChunkRoute.Count - 2);
        fromChunk = pathfinder.LastChunkRoute[boundaryIndex];
        toChunk = pathfinder.LastChunkRoute[boundaryIndex + 1];
        return true;
    }

    private static int CountCreatures()
    {
        int count = 0;

        foreach (var thing in ThingHandler.Things)
        {
            if (thing is Creature)
            {
                count++;
            }
        }

        return count;
    }

    private static void DrawChunkWireCube(Vector2Int chunkPos, int tilesPerChunk)
    {
        Vector2 center = GetChunkCenter(chunkPos);
        Gizmos.DrawWireCube(center, new Vector2(tilesPerChunk, tilesPerChunk));
    }

    private static Vector2 GetChunkCenter(Vector2Int chunkPos)
    {
        return chunkPos * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f);
    }

    private static bool ShouldDrawEdgeOnce(Vector2 from, Vector2 to)
    {
        if (from.x < to.x)
        {
            return true;
        }

        if (from.x > to.x)
        {
            return false;
        }

        return from.y <= to.y;
    }

    private static Color GetDebugNodeColor(NodeGraph node, Color fallbackColor)
    {
        if (node == null)
        {
            return fallbackColor;
        }

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

        return fallbackColor;
    }
}
