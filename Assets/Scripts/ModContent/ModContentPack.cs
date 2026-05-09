using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;
namespace ModContent
{
    public sealed class ModContentPack
    {
        public void LoadContent(ModMetaData metadata,bool isOfficial = false)
        {
            assemblyHolder.LoadAssembly(Path.Combine(metadata.Root.FullName, AssemblyFolderName));
            
            LoadTexture(metadata, isOfficial);
            LoadData(metadata);
        }

        private void LoadTexture(ModMetaData metadata, bool isOfficial)
        {
            string texturesPath = Path.Combine(metadata.Root.FullName, FilePathHandler.GetNameFolderByType.TryGetValue(typeof(Texture2D), out string folderName) ? folderName : "Textures");
            Debug.Log($"Loading textures from path: {texturesPath}");
            //Load textures from all directories under texturesPath
            if(Directory.Exists(texturesPath))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(texturesPath,"*",SearchOption.AllDirectories)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
                Debug.Log($"Found {files.Count()} texture files in mod: {metadata.Meta.modName}");
                foreach(var file in files)
                {
                    string relativePath = Path.GetRelativePath(texturesPath, file);
                    string key = Path.ChangeExtension(relativePath, null).Replace(Path.DirectorySeparatorChar, '/');
                    key = isOfficial ? $"Official/{key}" : $"Mods/{metadata.Meta.packageId}/{key}";
                    byte[] fileData = File.ReadAllBytes(file);
                    Texture2D texture = new Texture2D(2, 2);
                    if(texture.LoadImage(fileData))
                    {
                        texture.name = key;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load texture at path: {file}");
                    }
                    if(texture != null)
                    {
                        textures.AddContent(key, texture);
                        Debug.Log($"Loaded texture: {key} from mod: {metadata.Meta.modName}");
                    }
                }
            }else
            {
                Debug.LogWarning($"Textures folder not found for mod: {metadata.Meta.modName} at path: {texturesPath}");
            }
        }

        private void LoadData(ModMetaData metadata)
        {
            string dataPath = Path.Combine(metadata.Root.FullName, DataFolderName);
            if(!Directory.Exists(dataPath))
            {
                return;
            }

            //Load data from all directories under dataPath
            IEnumerable<string> files = Directory.EnumerateFiles(dataPath,"*.xml",SearchOption.AllDirectories);
            foreach(var file in files)
            {
                string content = File.ReadAllText(file);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(content);
                XmlNode root = doc.DocumentElement;
                if(root.Name != "Data")
                {
                    Debug.LogWarning($"Root node of XML file {file} is not 'Data'.");
                }
                if(root != null && root.HasChildNodes)
                {
                    foreach(XmlNode node in root.ChildNodes)
                    {
                        if(node.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }
                        System.Type type = TypeUtils.GetAllTypes().FirstOrDefault(t => t.Name == node.Name);
                        if(type == null)
                        {
                            Debug.LogError($"Type {node.Name} not found for XML node in file {file}.At node: {node.OuterXml}");
                            continue;
                        }
                        
                        var method = typeof(XmlLoader).GetMethod("DeserializeFromXml", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        var genericMethod = method.MakeGenericMethod(type);
                        object value = genericMethod.Invoke(null, new object[] { node, null, false });

                        RawData raw = value as RawData;

                        if(raw == null)
                        {
                            Debug.LogError($"Deserialized object from XML node is not of type RawData in file {file}. At node: {node.OuterXml}");
                            continue;
                        }

                        if(string.IsNullOrEmpty(raw.Id))
                        {
                            Debug.LogError($"RawData object deserialized from XML node has null or empty Id in file {file}. At node: {node.OuterXml}");
                            continue;
                        }

                        var addMethod = typeof(DatabaseThing)
                            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                            .Single(methodInfo =>
                                methodInfo.Name == "AddData" &&
                                methodInfo.IsGenericMethodDefinition &&
                                methodInfo.GetParameters().Length == 3 &&
                                methodInfo.GetParameters()[0].ParameterType == typeof(string) &&
                                methodInfo.GetParameters()[1].ParameterType.IsGenericParameter &&
                                methodInfo.GetParameters()[2].ParameterType == typeof(bool));
                        var genericAddMethod = addMethod.MakeGenericMethod(type);
                        genericAddMethod.Invoke(null, new object[] { raw.Id, value, false });
                    }
                }
            }
        }

        private ModAssemblyHolder assemblyHolder = new ModAssemblyHolder();
        private ContentHolder<Texture2D> textures = new ContentHolder<Texture2D>();

        const string DataFolderName = "Data";
        const string AssemblyFolderName = "Assembly";
        const string HelperDataFolderName = "HelperData";

        public static HashSet<string> imageExtensions = new()
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".psd",
        };
    }

}
