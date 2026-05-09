using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefineThing : MonoBehaviour
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
    public List<HelperComp> helperComps = new List<HelperComp>();

    public String ID => this.Id;
    public String LabelName => labelName;
    public String LabelDescription => labelDescription;

    public virtual void ConfigError()
    {
        if (string.IsNullOrEmpty(labelName))
        {
            Debug.LogError($"Label name is missing for {this.GetType().Name}");
        }
    }

    public virtual void Start()
    {
        Debug.Log($"Starting to apply definition for {this.GetType().Name} with ID: {Id}");
        foreach(var comp in helperComps)
        {
            comp.Init();
        }
        Debug.Log($"Finished applying definition for {this.GetType().Name} with ID: {Id}");
    }

    public T GetHelperComp<T>() where T : HelperComp
    {
        foreach(var comp in helperComps)
        {
            if(comp is T targetComp)
            {
                return targetComp;
            }
        }
        return null;
    }

    public void AddHelperComp(HelperComp comp)
    {
        if(comp == null)
        {
            Debug.LogError($"Cannot add null HelperComp to {this.GetType().Name}");
            return;
        }
        helperComps.Add(comp);
        comp.parent = this;
    }
    public void AddHelperComp<T>(T comp) where T : HelperComp
    {
        if(comp == null)
        {
            Debug.LogError($"Cannot add null HelperComp to {this.GetType().Name}");
            return;
        }
        helperComps.Add(comp);
        comp.parent = this;
    }
    public void SetDef(Define def)
    {
        this.def = def;
    }
}
