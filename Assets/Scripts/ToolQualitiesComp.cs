using System.Collections.Generic;
using UnityEngine;

public class ToolQualitiesComp : HelperComp
{
    public List<DefQuality> qualities = new List<DefQuality>();

    

    public class DefQuality
    {
        public string Id;
        public int value;
    }
}
