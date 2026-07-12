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
    private static HashSet<System.Type> registeredTypes;
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

    public static IReadOnlyCollection<System.Type> RegisteredTypes => registeredTypes;

    public static bool IsTypeCollectionRegistered(System.Type type)
    {
        return registeredTypes.Contains(type);
    }

    public static void AddRegisteredType(System.Type type)
    {
        if (!registeredTypes.Contains(type))
        {
            registeredTypes.Add(type);
        }
    }

    public static void RemoveRegisteredType(System.Type type)
    {
        if (registeredTypes.Contains(type))
        {
            registeredTypes.Remove(type);
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

    public static Sprite GetSquareSprite => Asset<Sprite>.Get("Square");
    public static Sprite GetCircleSprite => Asset<Sprite>.Get("Circle");
    public static Sprite GetMissingTexture => Asset<Sprite>.Get("debugempty_0");
}

public static class Asset<T>
{
    static Asset()
    {
        assets = new Dictionary<string, T>();
    }
    private static Dictionary<string, T> assets;

    public static IReadOnlyDictionary<string, T> Assets => assets;

    public static bool Register(string id, T asset, bool overwrite = false)
    {
        if(!GlobalAssets.IsTypeCollectionRegistered(typeof(T)))
        {
            GlobalAssets.AddRegisteredType(typeof(T));
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
                GlobalAssets.RemoveRegisteredType(typeof(T));
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
