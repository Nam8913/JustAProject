using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý thuộc tính cơ bản của Creature (Strength, Agility, Intelligence, etc.).
/// Đọc cấu hình từ AttributesCompProperties.
/// 
/// Mỗi thuộc tính có:
/// - BaseValue: giá trị cơ bản (từ XML)
/// - Modifier: bonus từ equipment, buff, debuff
/// - FinalValue: BaseValue + Modifier
/// </summary>
[Serializable]
public class AttributesComp : EntitiesComp
{
    private Dictionary<string, AttributeState> _attributes;

    /// <summary>
    /// Sự kiện khi giá trị thuộc tính thay đổi.
    /// Parameters: attributeName, oldValue, newValue
    /// </summary>
    public event Action<string, float, float> OnAttributeChanged;

    public override void Init()
    {
        _attributes = new Dictionary<string, AttributeState>();

        var attrProps = props as AttributesCompProperties;
        if (attrProps == null || attrProps.attributes == null)
        {
            return;
        }

        foreach (var attrData in attrProps.attributes)
        {
            _attributes[attrData.name] = new AttributeState
            {
                Name = attrData.name,
                BaseValue = attrData.baseValue,
                Modifier = 0f
            };
        }
    }

    /// <summary>
    /// Lấy giá trị cuối cùng của thuộc tính.
    /// </summary>
    public float GetAttribute(string name)
    {
        return _attributes.TryGetValue(name, out var attr) ? attr.FinalValue : 0f;
    }

    /// <summary>
    /// Lấy thuộc tính state (để kiểm tra chi tiết).
    /// </summary>
    public AttributeState GetAttributeState(string name)
    {
        return _attributes.TryGetValue(name, out var attr) ? attr : null;
    }

    /// <summary>
    /// Đặt giá trị cơ bản cho thuộc tính.
    /// </summary>
    public void SetBaseValue(string name, float value)
    {
        if (_attributes.TryGetValue(name, out var attr))
        {
            float oldValue = attr.FinalValue;
            attr.BaseValue = value;
            OnAttributeChanged?.Invoke(name, oldValue, attr.FinalValue);
        }
    }

    /// <summary>
    /// Thêm modifier cho thuộc tính.
    /// </summary>
    public void AddModifier(string name, float modifier)
    {
        if (_attributes.TryGetValue(name, out var attr))
        {
            float oldValue = attr.FinalValue;
            attr.Modifier += modifier;
            OnAttributeChanged?.Invoke(name, oldValue, attr.FinalValue);
        }
    }

    /// <summary>
    /// Xóa modifier khỏi thuộc tính.
    /// </summary>
    public void RemoveModifier(string name, float modifier)
    {
        if (_attributes.TryGetValue(name, out var attr))
        {
            float oldValue = attr.FinalValue;
            attr.Modifier -= modifier;
            OnAttributeChanged?.Invoke(name, oldValue, attr.FinalValue);
        }
    }

    /// <summary>
    /// Lấy tất cả thuộc tính (cho debug/UI).
    /// </summary>
    public IReadOnlyDictionary<string, AttributeState> GetAllAttributes()
    {
        return _attributes;
    }

    /// <summary>
    /// Lấy danh sách tên thuộc tính.
    /// </summary>
    public IEnumerable<string> GetAttributeNames()
    {
        return (_attributes?.Keys != null || _attributes.Keys.Count > 0) ? _attributes.Keys : Array.Empty<string>();
    }

    // Convenience properties
    public float Strength => GetAttribute("Strength");
    public float Agility => GetAttribute("Agility");
    public float Intelligence => GetAttribute("Intelligence");
    public float Perception => GetAttribute("Perception");
    public float Constitution => GetAttribute("Constitution");
    public float Charisma => GetAttribute("Charisma");
    public float Luck => GetAttribute("Luck");

    public string DebugString()
    {
        if (_attributes == null || _attributes.Count == 0)
        {
            return $"Attributes ({owner?.LabelName}): No attributes configured";
        }

        string result = $"Attributes ({owner?.LabelName}):\n";
        foreach (var attr in _attributes.Values)
        {
            string modStr = attr.Modifier != 0 ? $" (+{attr.Modifier:F1})" : "";
            result += $"  {attr.Name}: {attr.FinalValue:F1}{modStr}\n";
        }
        return result;
    }

    /// <summary>
    /// Runtime state cho một thuộc tính.
    /// </summary>
    public class AttributeState
    {
        public string Name;
        public float BaseValue;
        public float Modifier;
        public float FinalValue => BaseValue + Modifier;
    }
}
