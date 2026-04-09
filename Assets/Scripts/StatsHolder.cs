using System.Collections.Generic;
using UnityEngine;

public sealed class StatsHolder
{
    private Dictionary<string, Status> statusDictionary;

    public StatsHolder()
    {
        statusDictionary = new Dictionary<string, Status>();
    }

    public StatsHolder(params Status[] baseStatuses)
    {
        statusDictionary = new Dictionary<string, Status>();
        foreach (Status status in baseStatuses)
        {
            AddStatus(status);
        }
    }

    public Status GetStatus(string name)
    {
        if (statusDictionary.TryGetValue(name, out Status status))
        {
            return status;
        }
        else
        {
            Debug.LogWarning($"Status '{name}' not found.");
            return null;
        }
    }
    public void AddStatus(Status status)
    {
        if (statusDictionary.ContainsKey(status.Name))
        {
            Debug.LogWarning($"Status '{status.Name}' already exists. Use UpdateStatus to modify it.");
        }
        else
        {
            statusDictionary[status.Name] = status;
        }
    }

    public float GetStatusValue(string name, bool logWarningIfNotFound = true)
    {
        Status status = GetStatus(name);
        if (status == null && logWarningIfNotFound)
        {
            Debug.LogWarning($"Status '{name}' not found.");
        }
        return status != null ? status.Value : 0f;
    }

    public float HP
    {
        get => GetStatusValue("HP");
        set
        {
            Status hpStatus = GetStatus("HP");
            if (hpStatus != null)
            {
                hpStatus.Value = value;
            }
        }
    }

    public float MP
    {
        get => GetStatusValue("MP");
        set
        {
            Status mpStatus = GetStatus("MP");
            if (mpStatus != null)
            {
                mpStatus.Value = value;
            }
        }
    }

    public float AR
    {
        get => GetStatusValue("AR");
        set
        {
            Status arStatus = GetStatus("AR");
            if (arStatus != null)
            {
                arStatus.Value = value;
            }
        }
    }

    public float MR
    {
        get => GetStatusValue("MR");
        set
        {
            Status mrStatus = GetStatus("MR");
            if (mrStatus != null)
            {
                mrStatus.Value = value;
            }
        }
    }

    public float HPRegen
    {
        get => GetStatusValue("HPRegen");
        set
        {
            Status hpRegenStatus = GetStatus("HPRegen");
            if (hpRegenStatus != null)
            {
                hpRegenStatus.Value = value;
            }
        }
    }

    public float MPRegen
    {
        get => GetStatusValue("MPRegen");
        set
        {
            Status mpRegenStatus = GetStatus("MPRegen");
            if (mpRegenStatus != null)
            {
                mpRegenStatus.Value = value;
            }
        }
    }

    public float MS
    {
        get => GetStatusValue("MS");
        set
        {
            Status msStatus = GetStatus("MS");
            if (msStatus != null)
            {
                msStatus.Value = value;
            }
        }
    }

    public float AD
    {
        get => GetStatusValue("AD");
        set
        {
            Status adStatus = GetStatus("AD");
            if (adStatus != null)
            {
                adStatus.Value = value;
            }
        }
    }
    
    public float AS
    {
        get => GetStatusValue("AS");
        set
        {
            Status asStatus = GetStatus("AS");
            if (asStatus != null)
            {
                asStatus.Value = value;
            }
        }
    }

    public float AP
    {
        get => GetStatusValue("AP");
        set
        {
            Status apStatus = GetStatus("AP");
            if (apStatus != null)
            {
                apStatus.Value = value;
            }
        }
    }

    public float CriticalChance
    {
        get => GetStatusValue("CriticalChance");
        set
        {
            Status ccStatus = GetStatus("CriticalChance");
            if (ccStatus != null)
            {
                ccStatus.Value = value;
            }
        }
    }

    public float CriticalDamageBonus
    {
        get => GetStatusValue("CriticalDamageBonus");
        set
        {
            Status cdbStatus = GetStatus("CriticalDamageBonus");
            if (cdbStatus != null)
            {
                cdbStatus.Value = value;
            }
        }
    }

    public float CR
    {
        get => GetStatusValue("CR");
        set
        {
            Status crStatus = GetStatus("CR");
            if (crStatus != null)
            {
                crStatus.Value = value;
            }
        }
    }

    public float LifeSteal
    {
        get => GetStatusValue("LifeSteal");
        set
        {
            Status lsStatus = GetStatus("LifeSteal");
            if (lsStatus != null)
            {
                lsStatus.Value = value;
            }
        }
    }

    public float SpellVamp
    {
        get => GetStatusValue("SpellVamp");
        set
        {
            Status svStatus = GetStatus("SpellVamp");
            if (svStatus != null)
            {
                svStatus.Value = value;
            }
        }
    }

    public float ArmorPenetration
    {
        get => GetStatusValue("ArmorPenetration");
        set
        {
            Status apenStatus = GetStatus("ArmorPenetration");
            if (apenStatus != null)
            {
                apenStatus.Value = value;
            }
        }
    }

    public float MagicPenetration
    {
        get => GetStatusValue("MagicPenetration");
        set
        {
            Status mpenStatus = GetStatus("MagicPenetration");
            if (mpenStatus != null)
            {
                mpenStatus.Value = value;
            }
        }
    }

    public float AttackRange
    {
        get => GetStatusValue("AttackRange");
        set
        {
            Status arStatus = GetStatus("AttackRange");
            if (arStatus != null)
            {
                arStatus.Value = value;
            }
        }
    }

    public string DebugString()
    {
        string debugInfo = "Statuses:\n";
        if(statusDictionary.Count == 0)
        {
            debugInfo += "No statuses available.\n";
        }
        foreach (var status in statusDictionary.Values)
        {
            debugInfo += $"- {status.Name}: {status.Value}\n";
        }
        return debugInfo;
    }
}
