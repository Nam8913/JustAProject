using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Chunk : MonoBehaviour
{
    private Vector2Int chunkPosition;
    Dictionary<Vector2, Tile> tiles = new Dictionary<Vector2, Tile>();
    Dictionary<Vector2, GameObject> tileObjects = new Dictionary<Vector2, GameObject>();

    public bool IsInitialized { get; private set; }
    public event Action<Chunk> InitializedCallback;

    void Start()
    {
        StartCoroutine(initTiles());
    }

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

    IEnumerator initTiles()
    {
        float scale = 0.05f; // Adjust the scale for noise frequency
        int batchSize = 34; // Number of tiles to create per frame
        for (int x = chunkPosition.x * LocalTestValue.tilesPerChunk; x < (chunkPosition.x + 1) * LocalTestValue.tilesPerChunk; x++)
        {
            for (int y = chunkPosition.y * LocalTestValue.tilesPerChunk; y < (chunkPosition.y + 1) * LocalTestValue.tilesPerChunk; y++)
            {
                float moveCost = GameService.Noise.FBM(x * scale, y * scale); // Example move cost based on Perlin noise
                Tile tile = new Tile(new Vector2(x, y), this, moveCost);

                GameObject tileObject = new GameObject($"Tile:{x}|{y}");
                tileObject.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0.01f); // Center the tile object on the tile position
                tileObject.transform.SetParent(this.transform);
                tileObjects[tile.tilePosition] = tileObject;
                tiles[tile.tilePosition] = tile;

                SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
                //Texture2D texture2D = ResourcesHandler.GetTextureByName("dirt");
                //renderer.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 256);;
                renderer.sprite = LocalRefDefaultRS.GetSpriteByName("Square");

                if(tileObjects.Count % batchSize == 0)
                {
                    yield return null; // Wait for the next frame after creating a batch of tiles
                }

                tile.SetGameObject(tileObject);

                DebugTile debugTile = tileObject.AddComponent<DebugTile>();
                debugTile.tile = tile;
                debugTile.isWalkable = tile.isWalkable;
                debugTile.moveCost = tile.moveCost;
            }
        }

        IsInitialized = true;
        InitializedCallback?.Invoke(this);
    }

    public Tile GetTileAtPosition(Vector2 position)
    {
        if (tiles.TryGetValue(position, out Tile tile))
        {
            return tile;
        }
        return null; // Return null if the tile does not exist
    }

    public Tile GetTileAtWorldPosition(Vector2 position)
    {
        Vector2 tilePosition = new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.y));
        return GetTileAtPosition(tilePosition);
    }
}
