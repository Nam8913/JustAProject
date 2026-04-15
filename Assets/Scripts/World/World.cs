using UnityEngine;

public class World
{
    private string worldName;
    private int seed;

    public World(string worldName, int seed)
    {
        this.worldName = worldName;
        this.seed = seed;
        GameService.Game.RegisterWorld(this);
    }
}
