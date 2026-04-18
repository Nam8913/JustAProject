using System.IO;
using UnityEngine;

public static class ResourcesHandler
{
    static ContentHolder<Texture2D> textureHolder = new ContentHolder<Texture2D>();
    
    public static Texture2D GetTextureByName(string name)
    {
        if(textureHolder.TryGetContent(name, out Texture2D texture))
        {
            return texture;
        }
        else
        {
            //Try to load from resources folder
            string path = Path.Combine(FilePathHandler.GetFolderPathByType(typeof(Texture2D)), name);
            Texture2D loadedTexture = Resources.Load<Texture2D>(path);
            if(loadedTexture != null)
            {
                textureHolder.AddContent(name, loadedTexture);
                return loadedTexture;
            }
            else
            {
                Debug.LogWarning($"Texture with name {name} not found in Resources folder at path: {path}");
                return null;
            }
        }
    }

    public static Texture2D AddTexture(string name, Texture2D texture, bool overwriteIfExists = true)
    {
        if(textureHolder.TryGetContent(name, out Texture2D existingTexture))
        {
            Debug.LogWarning($"Texture with name {name} already exists. {(overwriteIfExists ? "Overwriting it." : "Not adding the new texture.")}");
        }
        textureHolder.AddContent(name, texture, overwriteIfExists);
        return texture;
    }
}
