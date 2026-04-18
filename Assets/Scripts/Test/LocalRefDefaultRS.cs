using System.Collections.Generic;
using UnityEngine;

public class LocalRefDefaultRS : MonoBehaviour
{
    [SerializeField] private List<Sprite> listSpriteRef = new List<Sprite>();
    
    [SerializeField] private List<Material> listMaterialRef = new List<Material>();
    private static Dictionary<string, Sprite> dictSprite = new Dictionary<string, Sprite>();
    private static Dictionary<string, Material> dictMaterial = new Dictionary<string, Material>();
    

    void Awake()
    {
        foreach(var item in listSpriteRef)
        {
            dictSprite.Add(item.name, item);
        }

        foreach(var item in listMaterialRef)
        {
            dictMaterial.Add(item.name, item);
        }
    }

    public static Sprite GetSpriteByName(string name)
    {
        if (dictSprite.TryGetValue(name, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"Sprite with name {name} not found in dictionary.");
            return null;
        }
    }

    public static Material GetMaterialByName(string name)
    {
        if (dictMaterial.TryGetValue(name, out Material material))
        {
            return material;
        }
        else
        {
            Debug.LogWarning($"Material with name {name} not found in dictionary.");
            return null;
        }
    }
}
