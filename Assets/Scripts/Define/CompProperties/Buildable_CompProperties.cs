using UnityEngine;
[System.Serializable]
public class Buildable_CompProperties : CompProperties
{
   public bool rotatable = false;
   public bool uninstallable = false;
   public bool repairable = false;
   public Vector2Int size = Vector2Int.one;
}
