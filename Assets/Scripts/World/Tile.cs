using System.Collections.Generic;
using UnityEngine;

public class Tile : IContainerOwner
{
    protected Chunk chunk;
    protected GameObject tileObject;
    public Vector2 tilePosition{get; private set;}
    
    public bool isWalkable {get; private set;} = true;
    public float moveCost { get; private set; } = 1f;

    private Container container;

    public Container GroundContainer
    {
        get
        {
            if (container == null)
            {
                // container = new Container(1000, float.PositiveInfinity, float.PositiveInfinity);
                Debug.LogError("Container is null. This should not happen as Tile constructor initializes it. Check Tile initialization.");
            }

            return container;
        }
    }

    public void SetGameObject(GameObject obj)
    {
        tileObject = obj;
    }

    public GameObject GetGameObject()
    {
        return tileObject;
    }

    public bool HasContainer => container != null;
    public bool HasItems => container != null && container.hasAnyItems;

    public bool TryGetExistingContainer(out Container targetContainer)
    {
        targetContainer = container;
        return targetContainer != null;
    }

    public Tile(Vector2 tilePosition, Chunk chunk, float moveCost = 1f)
    {
        this.tilePosition = tilePosition;
        this.chunk = chunk;
        this.moveCost = moveCost;

        container = new Container(1000, float.PositiveInfinity, float.PositiveInfinity);
    }

    public bool TryGetContainer(out Container container)
    {
        container = GroundContainer;
        return container != null;
    }
    public static List<Tile> GetAllTilesAroundPos(Vector2Int pos, float radius)
    {
        List<Tile> tiles = new List<Tile>();

        Vector2 center = new Vector2(pos.x, pos.y);
        int minX = Mathf.FloorToInt(center.x - radius);
        int maxX = Mathf.CeilToInt(center.x + radius);
        int minY = Mathf.FloorToInt(center.y - radius);
        int maxY = Mathf.CeilToInt(center.y + radius);

        WorldHandler worldHandler = GameService.GetWorldHandler();
        if (worldHandler != null)
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2 worldPosition = new Vector2(x, y);
                    Tile tile = worldHandler.GetTileAtWorldPosition(worldPosition);
                    if (tile == null)
                    {
                        continue;
                    }

                    if (!tile.TryGetExistingContainer(out Container tileContainer) || tileContainer == null)
                    {
                        continue;
                    }

                    int dx = (int)Mathf.Abs(x - center.x);
                    int dy = (int)Mathf.Abs(y - center.y);

                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        tiles.Add(tile);
                    }
                }
            }
        }

        return tiles;
    }
}