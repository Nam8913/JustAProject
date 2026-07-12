using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class GraphicData
{
    public static bool Is<T>(GraphicData graphicData , out T data) where T : GraphicData
    {
        data = null;
        if(graphicData is T tData)
        {
            data = tData;
            return true;
        }
        return false;
    }
    public static T GetGraphicData<T>(GraphicData graphicData) where T : GraphicData
    {
        if (graphicData is T data)
        {
            return data;
        }
        else
        {
            Debug.LogError($"GraphicData is not of type {typeof(T).Name}");
            return null;
        }
    }
}

public sealed class SingleGraphicData : GraphicData
{
    public GraphicMetaData metaData;
}

public sealed class MultiGraphicData : GraphicData
{
    public List<GraphicMetaData> metaData;
}

public sealed class AtlasGraphicData : GraphicData
{
    public string atlasPath;
    public List<GraphicMetaData> metaData;
}

[System.Serializable]
public class GraphicMetaData
{
    public string path;
    public float pixelsPerUnit;
    public Vector2 pivot;
    public Vector2 startPos;
    public Vector2 size; // width and height
    public Vector4 border = Vector4.zero;
    public uint extrude = 0u;
    public SpriteMeshType spriteMeshType = SpriteMeshType.Tight;
    public bool generateFallbackPhysicsShape = false;
}

