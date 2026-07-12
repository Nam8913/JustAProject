#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
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
            PostProcessGraphicData(metadata, isOfficial);
        }

        private void LoadTexture(ModMetaData metadata, bool isOfficial)
        {
            string texturesPath = Path.Combine(metadata.Root.FullName, FilePathHandler.GetNameFolderByType.TryGetValue(typeof(Texture2D), out string folderName) ? folderName : "Textures");
            #if DEBUG_LOG_FLAG && false
            Debug.Log($"Loading textures from path: {texturesPath}");
            #endif
            //Load textures from all directories under texturesPath
            if(Directory.Exists(texturesPath))
            {
                IEnumerable<string> files = Directory.EnumerateFiles(texturesPath,"*",SearchOption.AllDirectories)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));
                #if DEBUG_LOG_FLAG && false
                Debug.Log($"Found {files.Count()} texture files in mod: {metadata.Meta.modName}");
                #endif
                if(files.Count() > 0) textures = new ContentHolder<Texture2D>();
                foreach(var file in files)
                {
                    string relativePath = Path.GetRelativePath(texturesPath, file);
                    string key = Path.ChangeExtension(relativePath, null).Replace(Path.DirectorySeparatorChar, '/');
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
                        #if DEBUG_LOG_FLAG && false
                        Debug.Log($"Loaded texture: {key} from mod: {metadata.Meta.modName}");
                        #endif
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
                if(string.IsNullOrEmpty(content))
                {
                    Debug.LogWarning($"XML file at path {file} is empty. Skipping.");
                    continue;
                }
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
                        
                        DatabaseThing.SetIdWithPackageId(raw.Id, metadata.Meta.packageId);

                        if(value is Define define)
                        {
                            if(string.IsNullOrEmpty(define.Id))
                            {
                                continue;
                            }
                            defines.TryAdd(define.Id, define);
                            
                        }
                    }
                }
            }
        }

        private ModAssemblyHolder assemblyHolder = new ModAssemblyHolder();
        public ContentHolder<Texture2D> textures;
        private Dictionary<string, Define> defines = new Dictionary<string, Define>();

        /// <summary>
        /// Post-process: convert GraphicData → Sprite và register vào Asset<Sprite>.
        /// Chạy SAU KHI load xong textures và data.
        /// </summary>
        private void PostProcessGraphicData(ModMetaData metadata, bool isOfficial)
        {
            if (textures == null) return;

            string prefix = $"{metadata.Meta.packageId}:";

            if (defines == null) return;

            foreach (var kvp in defines)
            {
                if (kvp.Value is not Define define) continue;
                if (define.graphicData == null) continue;

                try
                {
                    
                    if (GraphicData.Is(define.graphicData, out SingleGraphicData singleGraphic))
                    {
                        ModAssets modAssets = GlobalAssets.GetModAssets(metadata.Meta.packageId);
                        Sprite sprite = modAssets.GetAsset<Sprite>(prefix + singleGraphic.metaData.path);
                        if(sprite == null)
                        {
                            Texture2D texture = modAssets.GetAsset<Texture2D>(prefix + singleGraphic.metaData.path);
                            if(texture == null)
                            {
                                Debug.LogError($"Texture not found for path: {prefix + singleGraphic.metaData.path} in mod: {metadata.Meta.modName} by request create graphicData by Def id: {define.Id} and type: {define.GetType().Name}");
                                sprite = GlobalAssets.GetMissingTexture;
                            }
                            else
                            {
                                sprite = Sprite.Create(texture, new Rect(singleGraphic.metaData.startPos.x, singleGraphic.metaData.startPos.y, singleGraphic.metaData.size.x, singleGraphic.metaData.size.y), singleGraphic.metaData.pivot, singleGraphic.metaData.pixelsPerUnit, singleGraphic.metaData.extrude, singleGraphic.metaData.spriteMeshType, singleGraphic.metaData.border, singleGraphic.metaData.generateFallbackPhysicsShape);
                            }
                            
                            if (sprite != null)
                            {
                                Asset<Sprite>.Register(prefix + singleGraphic.metaData.path, sprite, false);
                                #if DEBUG_LOG_FLAG && false
                                Debug.Log($"Registered sprite asset with id: {prefix + singleGraphic.metaData.path} from mod: {metadata.Meta.modName}");
                                #endif
                            }
                            else
                            {
                                Debug.LogError($"Failed to build sprite for '{define.Id}' with path '{singleGraphic.metaData.path}' in mod: {metadata.Meta.modName}");
                            }
                        }
                        continue;
                    }
                    else
                    {
                        
                        if (GraphicData.Is(define.graphicData, out MultiGraphicData multiGraphic))
                        {
                            List<GraphicMetaData> getGraphic = new List<GraphicMetaData>();
                            if (multiGraphic.metaData != null)
                            {
                                getGraphic.AddRange(multiGraphic.metaData);
                            }

                            foreach(var graphicMeta in getGraphic)
                            {
                                if(graphicMeta == null || string.IsNullOrEmpty(graphicMeta.path)) continue;

                                ModAssets modAssets = GlobalAssets.GetModAssets(metadata.Meta.packageId);
                                Sprite sprite = modAssets.GetAsset<Sprite>(prefix + graphicMeta.path);
                                if(sprite == null)
                                {
                                    Texture2D texture = modAssets.GetAsset<Texture2D>(prefix + graphicMeta.path);
                                    if(texture == null)
                                    {
                                        Debug.LogError($"Texture not found for path: {prefix + graphicMeta.path} in mod: {metadata.Meta.modName} by request create graphicData by Def id: {define.Id} and type: {define.GetType().Name}");
                                        sprite = GlobalAssets.GetMissingTexture;
                                    }
                                    else
                                    {
                                        sprite = Sprite.Create(texture, new Rect(graphicMeta.startPos.x, graphicMeta.startPos.y, graphicMeta.size.x, graphicMeta.size.y), graphicMeta.pivot, graphicMeta.pixelsPerUnit, graphicMeta.extrude, graphicMeta.spriteMeshType, graphicMeta.border, graphicMeta.generateFallbackPhysicsShape);
                                    }
                                    
                                    if (sprite != null)
                                    {
                                        Asset<Sprite>.Register(prefix + graphicMeta.path, sprite, false);
                                        #if DEBUG_LOG_FLAG && false
                                        Debug.Log($"Registered sprite asset with id: {prefix + graphicMeta.path} from mod: {metadata.Meta.modName}");
                                        #endif
                                    }
                                    else
                                    {
                                        Debug.LogError($"Failed to build sprite for '{define.Id}' with path '{graphicMeta.path}' in mod: {metadata.Meta.modName}");
                                    }
                                }
                            }
                        }
                        else if (GraphicData.Is(define.graphicData, out AtlasGraphicData atlasGraphic))
                        {
                            string atlasPath = atlasGraphic.atlasPath;
                            if(string.IsNullOrEmpty(atlasGraphic.atlasPath))
                            {
                                Debug.LogError($"AtlasGraphicData has null or empty atlasPath for {define.GetType().Name} with ID: {define.Id}");
                                if(atlasGraphic.metaData == null)
                                {
                                    Debug.LogError($"AtlasGraphicData has null metaData for {define.GetType().Name} with ID: {define.Id}");
                                    continue;
                                }else if(atlasGraphic.metaData.Count == 0)
                                {
                                    Debug.LogError($"AtlasGraphicData has empty metaData for {define.GetType().Name} with ID: {define.Id}");
                                    continue;
                                }
                                else if(string.IsNullOrEmpty(atlasGraphic.metaData.First().path))
                                {
                                    Debug.LogError($"AtlasGraphicData has null or empty path for {define.GetType().Name} with ID: {define.Id}");
                                    continue;
                                }
                                atlasPath = atlasGraphic.metaData.First().path;
                            }

                            List<GraphicMetaData> getGraphic = new List<GraphicMetaData>();
                            if (atlasGraphic.metaData != null)
                            {
                                getGraphic.AddRange(atlasGraphic.metaData);
                            }
                            ModAssets modAssets = GlobalAssets.GetModAssets(metadata.Meta.packageId);
                            foreach(var graphicMeta in getGraphic)
                            {
                                if(graphicMeta == null) continue;

                                Sprite sprite = modAssets.GetAsset<Sprite>(prefix + graphicMeta.path);
                                if(sprite == null)
                                {
                                    Texture2D texture = modAssets.GetAsset<Texture2D>(prefix + graphicMeta.path);
                                    if(texture == null)
                                    {
                                        Debug.LogError($"Texture not found for path: {prefix + graphicMeta.path} in mod: {metadata.Meta.modName} by request create graphicData by Def id: {define.Id} and type: {define.GetType().Name}");
                                        sprite = GlobalAssets.GetMissingTexture;
                                    }
                                    else
                                    {
                                        sprite = Sprite.Create(texture, new Rect(graphicMeta.startPos.x, graphicMeta.startPos.y, graphicMeta.size.x, graphicMeta.size.y), graphicMeta.pivot, graphicMeta.pixelsPerUnit, graphicMeta.extrude, graphicMeta.spriteMeshType, graphicMeta.border, graphicMeta.generateFallbackPhysicsShape);
                                    }
                                    
                                    if (sprite != null)
                                    {
                                        Asset<Sprite>.Register(prefix + graphicMeta.path, sprite, false);
                                        #if DEBUG_LOG_FLAG && false
                                        Debug.Log($"Registered sprite asset with id: {prefix + graphicMeta.path} from mod: {metadata.Meta.modName}");
                                        #endif
                                    }
                                    else
                                    {
                                        Debug.LogError($"Failed to build sprite for '{define.Id}' with path '{graphicMeta.path}' in mod: {metadata.Meta.modName}");
                                    }
                                }
                                continue;
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to build sprite for '{define.Id}': {ex.Message}");
                }
            }

            defines.Clear();
            defines = null;
        }

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
