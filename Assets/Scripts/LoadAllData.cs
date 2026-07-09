#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using ModContent;
public static class LoadAllData
{
    public static void LoadAll()
    {

        LoadDataFromNativeUnityResources();
        LoadDataFromMods();
    }

    public static void LoadDataFromNativeUnityResources()
    {
        
    }

    public static void LoadDataFromMods()
    {
        foreach(var assembly in TypeUtils.GetAllAssemblies())
        {
            TypeUtils.LoadTypesCachedFromAssembly(assembly);
        }
        using(DisposableStopwatch sw = new DisposableStopwatch())
        {
            ModsConfig.BuildModList();
            foreach(var mod in ModsConfig.EnabledModsInLoadOrder)
            {
                #if DEBUG_LOG_FLAG && false
                Debug.Log($"Loading data from mod: {mod.Meta.modName} ({mod.Meta.packageId})");
                #endif
                mod.LoadModContent();
            }
        }
        
    }
}
