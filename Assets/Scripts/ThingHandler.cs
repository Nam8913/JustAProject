using System.Collections.Generic;
using UnityEngine;

public static class ThingHandler
{
    static List<DefineThing> things = new List<DefineThing>();

    public static T CreateThing<T>() where T : DefineThing, new()
    {
        GameObject newObj = new GameObject(typeof(T).Name);
        T newThing = newObj.AddComponent<T>();
        newThing.ConfigError();
        things.Add(newThing);
        return newThing;
    }

    public static void RemoveThing(DefineThing thing)
    {
        if (things.Contains(thing))
        {
            things.Remove(thing);
            GameObject.Destroy(thing.gameObject);
        }
    }

    public static List<DefineThing> Things
    {
        get
        {
            return things;
        }
    }

    public static int GetCreatureCountInChunk(Vector2Int chunkPosition)
    {
        int count = 0;

        foreach (var thing in things)
        {
            if (thing is Creature creature && Chunk.GetChunkPosition(creature.transform.position) == chunkPosition)
            {
                count++;
            }
        }

        return count;
    }
}
