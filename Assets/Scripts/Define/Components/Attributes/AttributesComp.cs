using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;

/// <summary>
/// Serialized config data for a single attribute (from XML).
/// </summary>
[Serializable]
public class AttributeData
{
    public string name;
    public float baseValue = 10f;
}

/// <summary>
/// Runtime attribute value with base + modifier support.
/// </summary>
[Serializable]
public class AttributeValue
{
    public string Name { get; }
    public float BaseValue { get; set; }

    private float _modifier;
    public float Modifier
    {
        get => _modifier;
        set
        {
            _modifier = value;
            FinalValue = BaseValue + _modifier;
        }
    }

    public float FinalValue { get; private set; }

    public AttributeValue(string name, float baseValue)
    {
        Name = name;
        BaseValue = baseValue;
        _modifier = 0f;
        FinalValue = baseValue;
    }
}

/// <summary>
/// Manages creature base attributes: Strength, Agility, Intelligence,
/// Perception, Constitution, Charisma, Luck.
/// Each attribute has a base value (from XML) and a modifier (from equipment/buffs/debuffs).
/// </summary>
[Serializable]
public class AttributesComp : AttributeModule
{
    [SerializeField] private List<AttributeData> _attributeConfigs = new();
    private Dictionary<string, AttributeValue> _attributes = new();

    protected override string ModuleName => "Attributes";

    public IReadOnlyDictionary<string, AttributeValue> Attributes => _attributes;

    // --- Convenience properties ---
    public AttributeValue Strength => GetAttribute("Strength");
    public AttributeValue Agility => GetAttribute("Agility");
    public AttributeValue Intelligence => GetAttribute("Intelligence");
    public AttributeValue Perception => GetAttribute("Perception");
    public AttributeValue Constitution => GetAttribute("Constitution");
    public AttributeValue Charisma => GetAttribute("Charisma");
    public AttributeValue Luck => GetAttribute("Luck");

    // --- Lifecycle ---

    public override void Init()
    {
        base.Init();
        _attributes.Clear();
        foreach (var config in _attributeConfigs)
        {
            _attributes[config.name] = new AttributeValue(config.name, config.baseValue);
        }
    }

    // --- Attribute access ---

    public AttributeValue GetAttribute(string name)
    {
        _attributes.TryGetValue(name, out var attr);
        return attr;
    }

    public void SetBaseValue(string name, float value)
    {
        if (!_attributes.TryGetValue(name, out var attr)) return;
        float old = attr.FinalValue;
        attr.BaseValue = value;
        if (Math.Abs(attr.FinalValue - old) > 0.001f)
            NotifyAttributeChanged(name, old, attr.FinalValue);
    }

    public void AddModifier(string name, float mod)
    {
        if (!_attributes.TryGetValue(name, out var attr)) return;
        float old = attr.FinalValue;
        attr.Modifier += mod;
        if (Math.Abs(attr.FinalValue - old) > 0.001f)
            NotifyAttributeChanged(name, old, attr.FinalValue);
    }

    public void RemoveModifier(string name, float mod)
    {
        if (!_attributes.TryGetValue(name, out var attr)) return;
        float old = attr.FinalValue;
        attr.Modifier -= mod;
        if (Math.Abs(attr.FinalValue - old) > 0.001f)
            NotifyAttributeChanged(name, old, attr.FinalValue);
    }

    // --- XML ---

    public override void LoadFromXml(XmlNode node)
    {
        _attributeConfigs.Clear();
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.Name != "Attribute") continue;

            string attrName = child.Attributes?["name"]?.Value ?? "";
            float baseVal = float.TryParse(child.InnerText?.Trim(),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 10f;

            _attributeConfigs.Add(new AttributeData { name = attrName, baseValue = baseVal });
        }
    }

    // --- Debug ---

    public string DebugString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[AttributesComp]");
        foreach (var kvp in _attributes)
        {
            var a = kvp.Value;
            sb.AppendLine($"  {a.Name}: Base={a.BaseValue:F1} Mod={a.Modifier:F1} Final={a.FinalValue:F1}");
        }
        return sb.ToString();
    }
}
