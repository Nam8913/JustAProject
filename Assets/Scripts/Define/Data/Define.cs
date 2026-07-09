using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Define : BuildableData
{
    [SerializeReference]
    public List<CompProperties> compsProps = new List<CompProperties>();
    public int maxStackCount = 1;
    public float mass = 1f;
    public float volume = 1f;
    public float length = 1f;
    public bool destroyable = true;
    
    public GraphicData graphicData;
}