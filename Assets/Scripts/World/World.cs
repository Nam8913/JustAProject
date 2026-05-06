using UnityEngine;

public class World
{
    private string worldName;
    private int seed;

    public World(string worldName, int seed)
    {
        this.worldName = worldName;
        this.seed = seed;
    }

    public static void RegisterNewWorldToCurrentWorld(World world)
    {
        if(GameService.GetWorld() != null)
        {
            Debug.LogError("World is already registered. Multiple worlds are not supported.");
            return;
        }
        GameService.SetWorld(world);
    }

    public int Seed => seed;
    public string WorldName => worldName;
}
