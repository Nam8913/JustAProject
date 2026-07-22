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
        System.Type typeOfDifineThing = TypeUtils.TryGetType(typeName);
        if (typeOfDifineThing == null)
        {
            Debug.LogError($"Type not found for name: {typeName}");
            return null;
        }

        

        GameObject newObj = new GameObject();
        Type typeDef = DatabaseThing.GetTypeById(id);
        if(typeDef != null)
        {
            DatabaseThing.Store.TryGetValue(typeDef, out var dataDict);
            dataDict.TryGetValue(id, out var data);
            if(data is Define define)
            {
                newObj.name = define.label ?? define.Id;
                return DeloymentThingToObject(define, newObj);
            }
        }
        return null;
    }

    public static DefineThing DeloymentThingToObject(Define define, GameObject targetObject)
    {
        getThingById.TryGetValue(define.Id, out var typeName);
        if(string.IsNullOrEmpty(typeName))
        {
            Debug.LogError($"No mapping found for id: {define.Id}");
            return null;
        }

        System.Type typeOfDifineThing = TypeUtils.TryGetType(typeName);
        if (typeOfDifineThing == null)
        {
            Debug.LogError($"Type not found for name: {typeName}");
            return null;
        }

        DefineThing defineThing = targetObject.AddComponent(typeOfDifineThing) as DefineThing;
        thingsActive.Add(defineThing);

        defineThing.SetDef(define);
        MakeCompForDefineThing(defineThing, define);

        defineThing.ConfigError();

        if(define.tags != null && define.tags.Count > 0)
        {
            defineThing.Tags = new HashSet<string>(define.tags);
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

    private static void MakeCompForDefineThing(DefineThing defineThing, Define define)
    {
        foreach(var compProp in define.compsProps)
        {
            System.Type comp = compProp.compClass;
            bool flag1 = comp != null;
            if(!flag1)
            {
                Debug.LogError($"Comp class is not specified in definition for thing with id {define.Id}. Make sure the compClass is specified.");
                continue;
            }
            bool flag2 = typeof(EntitiesComp).IsAssignableFrom(comp);
            bool flag3 = comp.IsAbstract;
            if(flag1 && flag2 && !flag3)
            {
                EntitiesComp compInstance = (EntitiesComp)Activator.CreateInstance(comp);
                compInstance.props = compProp;
                compInstance.owner = defineThing;
                if(compInstance != null)
                {
                    defineThing.AddComp(compInstance);
                }
                else
                {
                    Debug.LogError($"Failed to create an instance of HelperComp type {comp.Name} for thing with id {define.Id}. Make sure the type has a parameterless constructor.");
                }
                compInstance.Init();
            }
            else
            {
                if(flag3)
                {
                    Debug.LogWarning($"Comp class {comp.Name} is abstract and cannot be instantiated for thing with id {define.Id}. Skipping this comp. Make sure the compClass is not abstract.");
                    continue;
                }else if(!flag1)
                {
                    Debug.LogError($"Comp class is not specified in definition for thing with id {define.Id}. Make sure the compClass is specified.");
                }else if(!flag2)
                {
                    Debug.LogError($"Comp class {comp.Name} does not inherit from HelperComp in definition for thing with id {define.Id}. Make sure the compClass inherits from HelperComp.");
                }else
                {
                    Debug.LogError($"Comp class {comp.Name} is invalid for thing with id {define.Id}. Make sure the compClass is a non-abstract class that inherits from HelperComp.");
                }
            }
        }
    }
}
