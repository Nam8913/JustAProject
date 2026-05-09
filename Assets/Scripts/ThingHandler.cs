using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class ThingHandler
{
    static Dictionary<string, string> getThingById = new Dictionary<string, string>(); // store the mapping of id and Type Of DefineThing, used for create thing by id
    static List<DefineThing> thingsActive = new List<DefineThing>();

    public static T CreateThing<T>() where T : DefineThing, new()
    {
        GameObject newObj = new GameObject(typeof(T).Name);
        T newThing = newObj.AddComponent<T>();
        newThing.ConfigError();
        thingsActive.Add(newThing);
        return newThing;
    }

    public static DefineThing CreateThingById(string id)
    {
        if (!getThingById.ContainsKey(id))
        {
            Debug.LogError($"No mapping found for id: {id}");
            return null;
        }

        string typeName = getThingById[id];
        System.Type typeOfDifineThing = System.Type.GetType(typeName);
        if (typeOfDifineThing == null)
        {
            Debug.LogError($"Type not found for name: {typeName}");
            return null;
        }

        

        GameObject newObj = new GameObject(id);
        DefineThing defineThing = newObj.AddComponent(typeOfDifineThing) as DefineThing;
        defineThing.ConfigError();
        thingsActive.Add(defineThing);

        Type typeDef = DatabaseThing.GetTypeById(id);
        if(typeDef != null)
        {
            DatabaseThing.Store.TryGetValue(typeDef, out var dataDict);
            dataDict.TryGetValue(id, out var data);
            if(data is Define define)
            {
                defineThing.SetDef(define);
                foreach(var compProp in define.compsProps)
                {
                    System.Type helperComp = compProp.compClass;
                    if(helperComp != null && typeof(HelperComp).IsAssignableFrom(helperComp))
                    {
                        HelperComp helperCompInstance = (HelperComp)Activator.CreateInstance(helperComp);
                        helperCompInstance.props = compProp;
                        helperCompInstance.parent = defineThing;
                        if(helperCompInstance != null)
                        {
                            defineThing.AddHelperComp(helperCompInstance);
                        }
                        else
                        {
                            Debug.LogError($"Failed to create an instance of HelperComp type {helperComp.Name} for thing with id {id}. Make sure the type has a parameterless constructor.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Comp class is null or not a valid HelperComp type in definition for thing with id {id}. Make sure the compClass is specified and inherits from HelperComp.");
                    }
                }
            }
        }

        return defineThing;
    }

    public static void RemoveThing(DefineThing thing)
    {
        if (thingsActive.Contains(thing))
        {
            thingsActive.Remove(thing);
            GameObject.Destroy(thing.gameObject);
        }
    }

    public static List<DefineThing> Things
    {
        get
        {
            return thingsActive;
        }
    }

    public static int GetCreatureCountInChunk(Vector2Int chunkPosition)
    {
        int count = 0;

        foreach (var thing in thingsActive)
        {
            if (thing is Creature creature && Chunk.GetChunkPosition(creature.transform.position) == chunkPosition)
            {
                count++;
            }
        }

        return count;
    }

    public static void AddThingMappingById(string id, string typeName, bool overwrite = false)
    {
        if (getThingById.ContainsKey(id))
        {
            if (overwrite)
            {
                getThingById[id] = typeName;
                return;
            }
            Debug.LogError($"A mapping for id {id} already exists. Use overwrite option to overwrite the existing mapping.");
            return;
        }
        getThingById[id] = typeName;
    }
}
