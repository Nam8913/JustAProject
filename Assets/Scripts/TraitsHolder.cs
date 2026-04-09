using System.Collections.Generic;
using UnityEngine;

public sealed class TraitsHolder
{
    private Dictionary<string, Trait> traitDictionary;

    public TraitsHolder()
    {
        traitDictionary = new Dictionary<string, Trait>();
    }

    public TraitsHolder(params Trait[] baseTraits)
    {
        traitDictionary = new Dictionary<string, Trait>();
        foreach (Trait trait in baseTraits)
        {
            AddTrait(trait);
        }
    }

    public Trait GetTrait(string name)
    {
        if (traitDictionary.TryGetValue(name, out Trait trait))
        {
            return trait;
        }
        else
        {
            Debug.LogWarning($"Trait '{name}' not found.");
            return null;
        }
    }

    public void AddTrait(Trait trait)
    {
        if (traitDictionary.ContainsKey(trait.Name))
        {
            Debug.LogWarning($"Trait '{trait.Name}' already exists. Use UpdateTrait to modify it.");
        }
        else
        {
            traitDictionary[trait.Name] = trait;
        }
    }

    public string DebugString()
    {
        string debugInfo = "Traits:\n";
        if(traitDictionary.Count == 0)
        {
            debugInfo += "- None\n";
        }
        foreach (var trait in traitDictionary.Values)
        {
            debugInfo += $"- {trait.Name}: {trait.Description}\n";
        }
        return debugInfo;
    }
}
