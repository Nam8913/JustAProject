using System.Collections.Generic;
using UnityEngine;

public static class DatabaseThing
{
    private static Dictionary<string, object> objectDataStore = new Dictionary<string, object>();
    private static Dictionary<string, System.Type> getTypeById = new Dictionary<string, System.Type>();
    private static Dictionary<System.Type, Dictionary<string, object>> typeDataStore = new Dictionary<System.Type, Dictionary<string, object>>();

    public static IReadOnlyDictionary<System.Type, Dictionary<string, object>> Store => typeDataStore;

    public static void AddData(string key, object data, bool overwrite = false)
    {
        if(objectDataStore.ContainsKey(key))
        {
            if(overwrite)
            {
                objectDataStore[key] = data;
                return;
            }
            Debug.LogError($"Data with key {key} already exists in the database.");
            return;
        }
        objectDataStore.Add(key, data);
    }

    public static void RemoveData(string key)
    {
        if(objectDataStore.ContainsKey(key))
        {
            objectDataStore.Remove(key);
        }
        else
        {
            Debug.LogError($"Data with key {key} not found in the database.");
        }
    }

    public static void AddData<T>(string key, T data, bool overwrite = false)
    {
        if(!typeDataStore.ContainsKey(typeof(T)))
        {
            typeDataStore[typeof(T)] = new Dictionary<string, object>();
        }
        if(typeDataStore[typeof(T)].ContainsKey(key))
        {
            if(overwrite)
            {
                typeDataStore[typeof(T)][key] = data;
                return;
            }
            Debug.LogError($"Data with key {key} already exists in the database.");
            return;
        }
        typeDataStore[typeof(T)].Add(key, data);
        getTypeById[key] = typeof(T);
        Debug.Log($"Added data of type {typeof(T).Name} with key {key} to the database.");
    }

    public static void RemoveData<T>(string key)
    {
        if(!typeDataStore.ContainsKey(typeof(T)))
        {
            Debug.LogError($"No data of type {typeof(T).Name} found in the database.");
            return;
        }
        if(typeDataStore[typeof(T)].ContainsKey(key))
        {
            typeDataStore[typeof(T)].Remove(key);
        }
        else
        {
            Debug.LogError($"Data with key {key} not found in the database.");
        }
        if (getTypeById.ContainsKey(key))
        {
            getTypeById.Remove(key);
        }
    }

    public static object GetData(string key)
    {
        if(objectDataStore.TryGetValue(key, out var data))
        {
            return data;
        }
        Debug.LogError($"Data with key {key} not found in the database.");
        return null;
    }

    public static T GetData<T>(string key)
    {
        if(!typeDataStore.ContainsKey(typeof(T)))
        {
            Debug.LogError($"No data of type {typeof(T).Name} found in the database.");
            return default;
        }
        if(typeDataStore[typeof(T)].TryGetValue(key, out var data))
        {
            if(data is T typedData)
            {
                return typedData;
            }
            Debug.LogError($"Data with key {key} is not of type {typeof(T).Name}.");
            return default;
        }
        Debug.LogError($"Data with key {key} not found in the database.");
        return default;
    }

    public static System.Type GetTypeById(string key)
    {
        if(getTypeById.TryGetValue(key, out var type))
        {
            return type;
        }
        Debug.LogError($"Data type for key {key} not found in the database.");
        return null;
    }
}
