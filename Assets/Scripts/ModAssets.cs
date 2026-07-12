#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System.Collections.Generic;
using UnityEngine;

namespace ModContent
{
    public class ModAssets
    {
        private ModMetaData modMetaData;
        public ModMetaData ModMetaData => modMetaData;
        public ModContentPack ModContentPack => modMetaData.ModContent;
        public ModAssets(ModMetaData modMetaData)
        {
            this.modMetaData = modMetaData;
            GlobalAssets.RegisterModAssets(modMetaData.Meta.packageId, this);
        }

        public void LoadContentFromModPackToAssets()
        {
            foreach(var item in ModContentPack.textures.contentDic)
            {
                string id = modMetaData.Meta.packageId + ":" + item.Key;
                Texture2D texture = item.Value;
                if(texture != null)
                {
                    Asset<Texture2D>.Register(id, texture, false);
                    #if DEBUG_LOG_FLAG && false
                    Debug.Log($"Registered texture asset with id: {id} from mod: {modMetaData.Meta.modName}");
                    #endif
                }
            }
        }

        public T GetAsset<T>(string id)
        {
            id = modMetaData.Meta.packageId + ":" + id;

            T rs = default(T);
            try
            {
                if(Asset<T>.Assets.TryGetValue(id, out T asset))
                {
                    rs = asset;
                }
                else
                {
                    Debug.LogError($"Asset of type {typeof(T).Name} with id {id} not found in mod {modMetaData.Meta.modName}");
                }
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError($"Asset of type {typeof(T).Name} with id {id} not found in mod {modMetaData.Meta.modName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error retrieving asset of type {typeof(T).Name} with id {id} in mod {modMetaData.Meta.modName}: {ex.Message}");
            }
            return rs;
        }

        public T RegisterAsset<T>(string id, T asset, bool overwrite = false)
        {
            id = modMetaData.Meta.packageId + ":" + id;
            Asset<T>.Register(id, asset, overwrite);
            return asset;
        }
    }
}