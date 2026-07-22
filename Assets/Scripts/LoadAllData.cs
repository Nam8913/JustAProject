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
        
        _ = ModsConfig.BuildModList();
        Debug.Log($"Enabled mods in load order: {string.Join(", ", ModsConfig.EnabledModsInLoadOrder)}");
        foreach(var mod in ModsConfig.EnabledModsInLoadOrder)
        {
            using(DisposableStopwatch sw = new DisposableStopwatch($"Loading data from mod: {mod.Meta.modName} ({mod.Meta.packageId})"))
            {
                mod.LoadModContent();
            }
        }
        
    }
}
