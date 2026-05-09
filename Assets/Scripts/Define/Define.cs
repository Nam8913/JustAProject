using System.Collections.Generic;

[System.Serializable]
public class Define : BuildableData
{
    public List<CompProperties> compsProps = new List<CompProperties>();
    public int maxStackCount = 1;
    public float mass = 1f;
    public float volume = 1f;
    public float length = 1f;
    public bool destroyable = true;

}