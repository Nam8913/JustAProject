using UnityEngine;

public class Tile
{
    protected Chunk chunk;
    public Vector2 tilePosition{get; private set;}
    
    public bool isWalkable {get; private set;} = true;
    public float moveCost { get; private set; } = 1f;

    public Tile(Vector2 tilePosition, Chunk chunk, float moveCost = 1f)
    {
        this.tilePosition = tilePosition;
        this.chunk = chunk;
        this.moveCost = moveCost;
    }
}