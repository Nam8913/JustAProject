using System.Collections.Generic;
using UnityEngine;

public class WorldHandler : MonoBehaviour
{
    private Dictionary<Vector2Int, Chunk> activeChunks = new Dictionary<Vector2Int, Chunk>();
    private int pendingChunkInitializationCount;

    public bool IsInitialized { get; private set; }

    void Awake()
    {
    }

    void Start()
    {
        IsInitialized = false;
        InitChunkAroundPlayer();
        NavService.Instance.PrewarmChunkGraphAround(LocalTestValue.focusAtPlayer);
    }

    void InitChunkAroundPlayer()
    {
        // This method will be responsible for initializing chunks around the player when the world is created
        // For now, we can just create a few chunks around the origin for testing purposes
        Vector2 playerPos = LocalTestValue.focusAtPlayer;
        int startX = Mathf.FloorToInt(playerPos.x / LocalTestValue.tilesPerChunk) - LocalTestValue.maxChunkWorkAroundPlayer;
        int endX = Mathf.FloorToInt(playerPos.x / LocalTestValue.tilesPerChunk) + LocalTestValue.maxChunkWorkAroundPlayer;
        int startY = Mathf.FloorToInt(playerPos.y / LocalTestValue.tilesPerChunk) - LocalTestValue.maxChunkWorkAroundPlayer;
        int endY = Mathf.FloorToInt(playerPos.y / LocalTestValue.tilesPerChunk) + LocalTestValue.maxChunkWorkAroundPlayer;
        for(int x = startX; x <= endX; x++)
        {
            for(int y = startY; y <= endY; y++)
            {
                Chunk chunk = Chunk.CreateChunk(new Vector2Int(x, y),this.gameObject);
                activeChunks[new Vector2Int(x, y)] = chunk;
                pendingChunkInitializationCount++;
                chunk.InitializedCallback += OnChunkInitialized;
            }
        }

        if (pendingChunkInitializationCount == 0)
        {
            IsInitialized = true;
        }
    }

    private void OnChunkInitialized(Chunk chunk)
    {
        if (chunk != null)
        {
            chunk.InitializedCallback -= OnChunkInitialized;
        }

        pendingChunkInitializationCount = Mathf.Max(0, pendingChunkInitializationCount - 1);
        if (pendingChunkInitializationCount == 0)
        {
            IsInitialized = true;
        }
    }

    public Chunk GetChunk(Vector2 position)
    {
        Vector2Int chunkPos = Chunk.GetChunkPosition(position);
        if (activeChunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            return chunk;
        }
        return null; // Or you could choose to create a new chunk here if it doesn't exist
    }

    public Chunk GetChunk(int chunkX, int chunkY)
    {
        Vector2Int chunkPos = new Vector2Int(chunkX, chunkY);
        if (activeChunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            return chunk;
        }
        return null; // Or you could choose to create a new chunk here if it doesn't exist
    }

    public Tile GetTileAtWorldPosition(Vector2 position)
    {
        Chunk chunk = GetChunk(position);
        if (chunk == null)
        {
            return null;
        }

        return chunk.GetTileAtWorldPosition(position);
    }

    public Dictionary<Vector2Int, Chunk> GetActiveChunks()
    {
        return activeChunks;
    }

    public List<Vector2Int> GetActiveChunkPositions()
    {
        return new List<Vector2Int>(activeChunks.Keys);
    }

    void OnDrawGizmos()
    {
        
    }

    void OnDestroy()
    {
        GameService.SetWorld(null);
        GameService.SetWorldHandler(null);
        GameService.SetNoise(null);
    } 
}

