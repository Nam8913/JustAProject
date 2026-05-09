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
                Debug.Log(meta.packageId);
                Debug.Log(meta.modName);
                Debug.Log(meta.version);
                Debug.Log(meta.description);
            }
        }

        public void LoadModContent()
        {
            modContentPack = new ModContentPack();
            modContentPack.LoadContent(this,isOfficial);
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