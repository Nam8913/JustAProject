#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ModContent
{
    public class ModMetaData
    {
        public ModMetaData(string localPath ,bool isOfficial = false)
        {
            root = new DirectoryInfo(localPath);
            this.isOfficial = isOfficial;
            Init();
        }

        void Init()
        {
            string infoFilePath = Path.Combine(root.FullName, InforFileName);
            if(!File.Exists(infoFilePath))
            {
                Debug.LogError($"Mod info file not found at path: {infoFilePath}");
                return;
            }
            meta = XmlLoader.LoadFromXml<ModMetaDataInternal>(infoFilePath);
            if(meta != null)
            {
                #if DEBUG_LOG_FLAG && false && false
                Debug.Log($"Loaded mod metadata from {infoFilePath}");
                Debug.Log(meta.packageId);
                Debug.Log(meta.modName);
                Debug.Log(meta.version);
                Debug.Log(meta.description);
                #endif
            }
        }

        public void LoadModContent()
        {
            ModAssets modAssets = new ModAssets(this);
            modContentPack = new ModContentPack();
            modContentPack.LoadContent(this,isOfficial);
            modAssets.LoadContentFromModPackToAssets();
        }

        private DirectoryInfo root;
        private Texture2D previewImage;
        private ModContentPack modContentPack;

        private bool isOfficial = false;

        private ModMetaDataInternal meta = new ModMetaDataInternal();

        public DirectoryInfo Root => root;
        public Texture2D PreviewImage => previewImage;
        public ModMetaDataInternal Meta => meta;
        public ModContentPack ModContent => modContentPack;

        const string InforFileName = "ModInfo.xml";

        [System.Serializable]
        public class ModMetaDataInternal
        {
            public string packageId = "";
            public string modName = "";
            public string version = "";
            public string description = "No description provided.";

            public List<string> loadBefore = new List<string>();
            public List<string> loadAfter = new List<string>();
            public List<string> incompatibleWith = new List<string>();
            public List<string> dependencies = new List<string>();

        }
    }
}