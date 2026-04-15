using System.Collections.Generic;
using UnityEngine;

public sealed class NavService
{
    private static readonly object _lock = new object();
    private static NavService instance;

    private readonly HPAPathfinder hpaPathfinder;

    public HPAPathfinder Pathfinder => hpaPathfinder;

    public static NavService Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new NavService();
                    }
                }
            }

            return instance;
        }
    }

    private NavService()
    {
        hpaPathfinder = new HPAPathfinder();
    }

    public void PrewarmChunkGraphAround(Vector2 playerPosition)
    {
        hpaPathfinder.PrewarmChunkGraphAround(playerPosition);
    }

    public List<Vector2> GetPathFromTo(Vector2 startPosition, Vector2 goalPosition)
    {
        return hpaPathfinder.GetPathFromTo(startPosition, goalPosition);
    }

    public List<Vector2> GetPathFromTo(Creature creature, Vector2 goalPosition)
    {
        if (creature == null)
        {
            return null;
        }

        return hpaPathfinder.GetPathFromTo(creature.WorldPosition, goalPosition);
    }

    public void ReleaseChunkGraphIfUnused(Vector2Int chunkPosition)
    {
        hpaPathfinder.ReleaseChunkGraphIfUnused(chunkPosition);
    }
}