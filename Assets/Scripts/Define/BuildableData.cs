using System.Collections.Generic;

[System.Serializable]
public class BuildableData : RawData
{
    public string name;
    public string label;
    public string description;
    public List<string> tags = new List<string>();
}
