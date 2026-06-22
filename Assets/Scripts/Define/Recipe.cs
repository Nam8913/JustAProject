using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public sealed class Recipe : BuildableData
{
    public string category;
    public string result;
    public float timeCraft = 1f; // Time required to craft the item, in seconds
    public List<RecipeComponent> components = new List<RecipeComponent>();
    public List<RequireToolQuality> qualities = new List<RequireToolQuality>();

    [Serializable]
    public class RecipeComponent
    {
        public List<ComponentOption> Options = new List<ComponentOption>();
    }
}


