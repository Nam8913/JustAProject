#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefineThing : MonoBehaviour, IContainerOwner
{
    [SerializeField]
    protected string Id;
    [SerializeField]
    protected string labelName;
    [SerializeField]
    [TextArea(3, 10)]
    protected string labelDescription;

    [SerializeField]
    public Define def;
    [SerializeField]
    [SerializeReference]
    public List<EntitiesComp> components = new List<EntitiesComp>();
    [SerializeField]
    private string modPackageId = string.Empty;

    public String ID => this.Id;
    public String LabelName => labelName;
    public String LabelDescription => labelDescription;
    public String ModPackageId => modPackageId;
    public ModContent.ModAssets LocalPackAssets => GlobalAssets.GetModAssets(modPackageId);

    public virtual void ConfigError()
    {
        if(def == null)
        {
            Debug.LogError($"Definition is missing for {this.GetType().Name} with ID: {Id}");
        }
        if (string.IsNullOrEmpty(labelName) && def.label != null)
        {
            labelName = def.label;
        }
        if(string.IsNullOrEmpty(labelName))
        {
            Debug.LogError($"Label name is missing for {this.GetType().Name} with ID: {Id}");
        }
    }

    public virtual void Start()
    {
        #if DEBUG_LOG_FLAG && false
        Debug.Log($"Starting to apply definition for {this.GetType().Name} with ID: {Id}");
        #endif
        foreach(var comp in components)
        {
            comp.Init();
        }
        #if DEBUG_LOG_FLAG && false
        Debug.Log($"Finished applying definition for {this.GetType().Name} with ID: {Id}");
        #endif
    }

    public virtual void Update()
    {
        if(components.Count > 0)
        {
            foreach(var comp in components)
            {
                comp.Update();
            }
        }
    }

    public virtual void FixedUpdate()
    {
        if(components.Count > 0)
        {
            foreach(var comp in components)
            {
                comp.FixedUpdate();
            }
        }
        
    }

    public virtual void LateUpdate()
    {
        if(components.Count > 0)
        {
            foreach(var comp in components)
            {
                comp.LateUpdate();
            }
        }
    }

    public T GetComp<T>() where T : EntitiesComp
    {
        foreach(var comp in components)
        {
            if(comp is T targetComp)
            {
                return targetComp;
            }
        }
        return null;
    }

    public virtual bool TryGetContainer(out Container container)
    {
        ProvideContainer_Comp containerComp = GetComp<ProvideContainer_Comp>();
        if (containerComp != null && containerComp.TryGetContainer(out container))
        {
            return true;
        }

        container = null;
        return false;
    }

    public void AddComp(EntitiesComp comp)
    {
        if(comp == null)
        {
            Debug.LogError($"Cannot add null HelperComp to {this.GetType().Name}");
            return;
        }
        components.Add(comp);
        comp.owner = this;
    }
    public void AddComp<T>(T comp) where T : EntitiesComp
    {
        if(comp == null)
        {
            Debug.LogError($"Cannot add null HelperComp to {this.GetType().Name}");
            return;
        }
        components.Add(comp);
        comp.owner = this;
    }
    public void SetDef(Define def)
    {
        if(def == null)
        {
            Debug.LogError($"Cannot set null definition for {this.GetType().Name} with ID: {Id}");
            return;
        }
        this.def = def;
        this.Id = def.Id;
        this.labelName = def.label;
        this.labelDescription = def.description;
        this.modPackageId = DatabaseThing.GetPackageIdById(def.Id);

    }
}
