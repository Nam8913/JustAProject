using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FilePathHandler
{
    public static string GetDataFolderPath
    {
        get
        {
            if(saveDataPath == null)
            {
                DirectoryInfo dir = new DirectoryInfo(fileExecutionPath);
                if(Application.isEditor)
                {
                    saveDataPath = dir.FullName;
                }else
                {
                    saveDataPath = persistentDataPath;
                }

                DirectoryInfo dir2 = new DirectoryInfo(saveDataPath);
                if(!dir2.Exists)
                {
                    dir2.Create();
                }
            }
            return saveDataPath;
        }
    }

    private static string FolderUnderSaveDataPath(string folderName)
    {
        string folderPath = Path.Combine(GetDataFolderPath, folderName);
        DirectoryInfo dir = new DirectoryInfo(folderPath);
        if(!dir.Exists)
        {
            dir.Create();
        }
        return folderPath;
    }

    private static string GetOrCreateModsFolder(string folderName)
    {
        string folderPath = Path.Combine(fileExecutionPath, folderName);
        DirectoryInfo dir = new DirectoryInfo(folderPath);
        if(!dir.Exists)
        {
            dir.Create();
        }
        return folderPath;
    }

    public static string GetFolderPathByType(System.Type type , string nameIfNotFound = null)
    {
        if(GetNameFolderByType.TryGetValue(type, out string folderName))
        {
            return folderName;
        }
        else
        {
            if(nameIfNotFound != null)
            {
                return nameIfNotFound;
            }
            Debug.LogError($"Type {type} not found in GetNameFolderByType dictionary.");
            return null;
        }
    }

    public static Dictionary <System.Type, string> GetNameFolderByType = new Dictionary<System.Type, string>()
    {
        {typeof(Texture2D), "Textures"},
        {typeof(AudioClip), "Audio"},
        {typeof(Material), "Materials"},
    };

    public static string ModsFolderPath => GetOrCreateModsFolder("Mods");
    public static string OfficialDataFolderPath => GetOrCreateModsFolder("Official");

    public static string ConfigFolderPath => FolderUnderSaveDataPath("Config");
    public static string SavesFolderPath => FolderUnderSaveDataPath("Saves");
    public static string LogsFolderPath => FolderUnderSaveDataPath("Logs");

    public static string SettingsFilePath => Path.Combine(ConfigFolderPath, "Settings.xml");

    private static string saveDataPath;

    private static string fileExecutionPath = Application.dataPath;
    private static string streamingAssetsPath = Application.streamingAssetsPath;
    private static string persistentDataPath = Application.persistentDataPath;
}
