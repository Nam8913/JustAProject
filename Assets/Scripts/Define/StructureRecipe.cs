using System;
using System.Collections.Generic;

[Serializable]
public sealed class StructureRecipe : BuildableData
{
    public string category;
    public string result;
    public float timeCraft = 1f; // Time required to craft the item, in seconds
    public List<StructureComponent> components = new List<StructureComponent>();
    public List<RequireToolQuality> qualities = new List<RequireToolQuality>();

    [Serializable]
    public class StructureComponent
    {
        public List<ComponentOption> Options = new List<ComponentOption>();
    }
}
