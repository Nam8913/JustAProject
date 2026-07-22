#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ModContent
{
    public static class ModsConfig
    {
        static ModsConfig()
        {
            // Load config from file
            string configPath = System.IO.Path.Combine(FilePathHandler.ConfigFolderPath, "ModsList.xml");
            if(!File.Exists(configPath))
            {
                Debug.Log("Mods config file not found, creating default one.");
                CreateDefaultConfig(configPath);
            }
            data = XmlLoader.LoadFromXml<ModsConfigData>(configPath);
            if(data == null)
            {
                Debug.LogError("Failed to load mods config data, try to create a new one.");
                File.Delete(configPath);
                CreateDefaultConfig(configPath);
                data = XmlLoader.LoadFromXml<ModsConfigData>(configPath);
                if(data == null)            {
                    Debug.LogError("Failed to load mods config data after creating default one, there must be something wrong with the file system or the XmlLoader.");
                }
                data = new ModsConfigData()
                {
                    activeMods = new List<string>()
                    {
                        "Official.Core"
                    }
                };
            }

            #if DEBUG_LOG_FLAG && false
            if(data != null)
            {
                foreach(var mod in data.activeMods)
                {
                    Debug.Log($"Active mod from config: {mod}");
                }
            }
            #endif
        }

        public static void CreateDefaultConfig(string path)
        {
            ModsConfigData defaultData = new ModsConfigData();
            File.Create(path).Close(); // Create an empty file
            using(StreamWriter wr = new StreamWriter(path))
            {
                wr.WriteLine("<ModsConfigData>");
                wr.WriteLine("  <activeMods>");
                wr.WriteLine("    <li>Official.Core</li>");
                wr.WriteLine("  </activeMods>");
                wr.WriteLine("</ModsConfigData>");
            }
        }

        public static async System.Threading.Tasks.Task BuildModList()
        {
            allModsCached.Clear();
            enabledModsInLoadOrderCached.Clear();

            // Load all mods from the Official folder
            Task officialModsTask = LoadOfficialModsTask();
            
            // Load all mods from the Mods folder
            Task modsTask = LoadModsTask();
            
            await Task.WhenAll(officialModsTask, modsTask);
        }

        static async Task LoadOfficialModsTask()
        {
            using(DisposableStopwatch sw = new DisposableStopwatch("ModsConfig.BuildOfficialModList"))
            {
                foreach(var modPath in from d in new DirectoryInfo(FilePathHandler.OfficialDataFolderPath).GetDirectories() select d.FullName)
                {
                    ModMetaData meta = new ModMetaData(modPath, isOfficial: true);
                    allModsCached.Add(meta);

                    if(data.activeMods.Contains(meta.Meta.packageId))
                    {
                        enabledModsInLoadOrderCached.Add(meta);
                    }
                }
            }
            await Task.CompletedTask;
        }
        static async Task LoadModsTask()
        {
            using(DisposableStopwatch sw = new DisposableStopwatch("ModsConfig.BuildModList"))
            {
                foreach(var modPath in from d in new DirectoryInfo(FilePathHandler.ModsFolderPath).GetDirectories() select d.FullName)
                {
                    ModMetaData meta = new ModMetaData(modPath, isOfficial: false);
                    allModsCached.Add(meta);

                    if(data.activeMods.Contains(meta.Meta.packageId))
                    {
                        enabledModsInLoadOrderCached.Add(meta);
                    }
                }
            }
            await Task.CompletedTask;
        }

        private static ModsConfigData data = new ModsConfigData();
        private static List<ModMetaData> allModsCached = new List<ModMetaData>();
        private static List<ModMetaData> enabledModsInLoadOrderCached = new List<ModMetaData>();

        public static IReadOnlyList<ModMetaData> AllMods => allModsCached;
        public static IReadOnlyList<ModMetaData> EnabledModsInLoadOrder => enabledModsInLoadOrderCached;

        public class ModsConfigData
        {
            public List<string> activeMods = new List<string>();
        }
    }
}