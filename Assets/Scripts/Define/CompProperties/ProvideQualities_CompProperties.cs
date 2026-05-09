using System.Collections.Generic;
using UnityEngine;

public class ProvideQualities_CompProperties : CompProperties
{
    public List<ProvidedQuality> toolqualities = new List<ProvidedQuality>();
    public class ProvidedQuality
    {
        public string qualityId;
        public int level;
    }
}
