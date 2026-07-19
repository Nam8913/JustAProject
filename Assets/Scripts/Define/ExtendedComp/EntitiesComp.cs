using UnityEngine;
[System.Serializable]
public abstract class EntitiesComp
{
    [HideInInspector]
    public DefineThing owner;
    [HideInInspector]
    public CompProperties props;

    public Define Def => owner.def;

    public virtual void Init()
    {
        
    }

    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }

    public virtual void LateUpdate()
    {
    }
}
