using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public abstract class GraphicData
{
}

public sealed class SingleGraphicData : GraphicData
{
    public GraphicMetaData metaData;
}

public sealed class MultiGraphicData : GraphicData
{
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

