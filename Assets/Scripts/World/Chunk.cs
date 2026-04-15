using UnityEngine;

public class Chunk : MonoBehaviour
{
    private Vector2Int chunkPosition;

    public static Chunk CreateChunk(Vector2Int chunkPosition, GameObject parent = null)
    {
        GameObject chunkObject = new GameObject($"Chunk:{chunkPosition.x}|{chunkPosition.y}");
        Chunk chunk = chunkObject.AddComponent<Chunk>();
        chunk.chunkPosition = chunkPosition;
        chunkObject.transform.position = new Vector2(chunkPosition.x * LocalTestValue.tilesPerChunk, chunkPosition.y * LocalTestValue.tilesPerChunk);
        if (parent != null)
        {
            chunkObject.transform.SetParent(parent.transform);
        }
        // Additional initialization for the chunk can be done here
        return chunk;
    }

    public static Vector2Int GetChunkPosition(Vector2 position)
    {
        int chunkX = Mathf.FloorToInt(position.x / LocalTestValue.tilesPerChunk);
        int chunkY = Mathf.FloorToInt(position.y / LocalTestValue.tilesPerChunk);
        Vector2Int chunkPos = new Vector2Int(chunkX, chunkY);
        return chunkPos;
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireCube(new Vector2(chunkPosition.x * LocalTestValue.tilesPerChunk + LocalTestValue.tilesPerChunk / 2, chunkPosition.y * LocalTestValue.tilesPerChunk + LocalTestValue.tilesPerChunk / 2), new Vector2(LocalTestValue.tilesPerChunk, LocalTestValue.tilesPerChunk));
    
    //     //Draw tiles
    //     Gizmos.color = Color.gray;
    //     for (int x = 0; x < LocalTestValue.tilesPerChunk; x++)
    //     {
    //         for (int y = 0; y < LocalTestValue.tilesPerChunk; y++)
    //         {
    //             //Gizmos.DrawWireCube(new Vector2(chunkPosition.x * LocalTestValue.tilesPerChunk + x + 0.5f, chunkPosition.y * LocalTestValue.tilesPerChunk + y + 0.5f), Vector2.one);
    //             Gizmos.DrawWireSphere(new Vector2(chunkPosition.x * LocalTestValue.tilesPerChunk + x + 0.5f, chunkPosition.y * LocalTestValue.tilesPerChunk + y + 0.5f), 0.1f);
    //         }
    //     }
    // }
}
