#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System.Collections.Generic;
using UnityEngine;

public static class GlobalAssets
{
    static GlobalAssets()
    {
        // Initialize the mod asset mapping dictionary
        modAssetMapping = new Dictionary<string, ModContent.ModAssets>();
    }
    private static Dictionary<string, ModContent.ModAssets> modAssetMapping;
    private static List<System.Type> registeredTypes;
    private static void GetInformationFromAssetTypes()
    {
        foreach (var type in registeredTypes)
    {
        // Lấy số lượng assets của type này thông qua reflection
        var assetsProperty = typeof(Asset<>)
            .MakeGenericType(type)
            .GetProperty("Assets", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        
        var assets = assetsProperty?.GetValue(null);
        var countProperty = assets?.GetType().GetProperty("Count");
        int count = countProperty != null ? (int)countProperty.GetValue(assets) : 0;

        #if DEBUG_LOG_FLAG && false
        Debug.Log($"Registered Type: {type.FullName} - Asset Count: {count}");
        #endif
    }

    }

    public static List<System.Type> RegisteredTypes
    {
        get
        {
            if (registeredTypes == null)
            {
                registeredTypes = new List<System.Type>();
            }
            return registeredTypes;
        }
    }

    public static void RegisterModAssets(string packageId, ModContent.ModAssets modAssets)
    {
        if (!modAssetMapping.ContainsKey(packageId))
        {
            modAssetMapping.Add(packageId, modAssets);
        }
        else
        {
            Debug.LogWarning($"Mod assets for package ID {packageId} are already registered.");
        }
    }

    public static ModContent.ModAssets GetModAssets(string packageId)
    {
        if (modAssetMapping.TryGetValue(packageId, out var modAssets))
        {
            return modAssets;
        }
        else
        {
            Debug.LogWarning($"No mod assets found for package ID {packageId}.");
            return null;
        }
    }
}

public static class Asset<T>
{
    private static Dictionary<string, T> assets = new Dictionary<string, T>();

    public static IReadOnlyDictionary<string, T> Assets => assets;

    public static bool Register(string id, T asset, bool overwrite = false)
    {
        if(!GlobalAssets.RegisteredTypes.Contains(typeof(T)))
        {
            GlobalAssets.RegisteredTypes.Add(typeof(T));
        }

        if (!assets.ContainsKey(id))
        {
            assets.Add(id, asset);
        }
        else
        {
            if (overwrite)
            {
                assets[id] = asset;
                return true;
            }
            else
            {
                Debug.LogWarning($"Asset with ID {id} is already registered.");
                return false;
            }
        }
        return true;
    }

    public static void Unregister(string id)
    {
        if (assets.ContainsKey(id))
        {
            assets.Remove(id);
            if(assets.Count == 0)
            {
                GlobalAssets.RegisteredTypes.Remove(typeof(T));
            }
        }
        else
        {
            Debug.LogWarning($"No asset found with ID {id} to unregister.");
        }
    }

    public static T Get(string id)
    {
        if (assets.TryGetValue(id, out var asset))
        {
            return asset;
        }
        else
        {
            Debug.LogWarning($"No asset found with ID {id}.");
            return default(T);
        }
    }
}
