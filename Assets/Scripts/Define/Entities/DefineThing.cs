#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefineThing : MonoBehaviour, IContainerOwner
{
    [Header("General Info")]
    [SerializeField]
    protected string Id;
    [SerializeField]
    protected string labelName;
    [SerializeField]
    [TextArea(3, 10)]
    protected string labelDescription;

    [Header("Defs")]
    [SerializeField]
    public Define def;
    [SerializeField]
    [SerializeReference]
    public List<EntitiesComp> components = new List<EntitiesComp>();
    
    [Header("Components")]
    [SerializeField]
    public SpriteRenderer spriteRenderer;
    [SerializeField]
    public Collider2D mainCollider;
    [SerializeField]
    public Rigidbody2D rbg2d;

    [Header("Other")]
    [SerializeField]
    private string modPackageId = string.Empty;
    [SerializeField]
    private HashSet<string> tags = new HashSet<string>();

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

    public virtual void Setup(){}

    public T GetProperty<T>() where T : CompProperties
    {
        if(def != null && def.compsProps != null)
        {
            CompProperties comp = def.compsProps.Find(x => x is T);
            if(comp is T typed)
            {
                return typed;
            }
        }
        return null;
    }

    public bool HasProperty<T>() where T : CompProperties
    {
        if(def != null && def.compsProps != null)
        {
            return def.compsProps.Exists(x => x is T);
        }
        return false;
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
    
    public void RemoveComp(EntitiesComp comp)
    {
        if(comp == null)
        {
            Debug.LogError($"Cannot remove null HelperComp from {this.GetType().Name}");
            return;
        }
        components.Remove(comp);
        comp.owner = null;
    }

    public void RemoveComp<T>() where T : EntitiesComp
    {
        EntitiesComp compToRemove = GetComp<T>();
        if(compToRemove != null)
        {
            components.Remove(compToRemove);
            compToRemove.owner = null;
            compToRemove = null;
        }
    }

    public HashSet<string> Tags
    {
        get
        {
            return tags;
        }
        set
        {
            tags = value;
        }
    }

    public bool HasTag(string tag)
    {
        if(tags != null && tags.Count > 0)
        {
            return tags.Contains(tag);
        }
        return false;
    }

    public void AddTag(string tag)
    {
        if(!string.IsNullOrEmpty(tag))
        {
            tags.Add(tag);
        }
    }

    public void RemoveTag(string tag)
    {
        if(!string.IsNullOrEmpty(tag))
        {
            tags.Remove(tag);
        }
    }

    // ── State helpers (delegates to StateComp) ──

    public string GetState()
    {
        var state = GetComp<StateComp>();
        if(state == null)
        {
            UnityEngine.Debug.LogWarning($"StateComp is missing for {this.GetType().Name} with ID: {Id}");
        }
        return state?.GetState();
    }

    public void SetState(string newState)
    {
        var state = GetComp<StateComp>();
        if (state != null)
        {
            state.SetState(newState);
        }
    }

    public bool IsInState(string state)
    {
        var stateComp = GetComp<StateComp>();
        return stateComp != null && stateComp.IsInState(state);
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

        this.Setup();

        this.spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        this.rbg2d = this.gameObject.GetComponent<Rigidbody2D>();
        this.mainCollider = this.gameObject.GetComponent<Collider2D>();
    }
}
