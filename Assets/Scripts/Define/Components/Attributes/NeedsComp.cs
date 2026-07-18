using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[Serializable]
public class NeedData
{
    public string name;
    public float maxValue = 100f;
    public float decayRate = 0.5f;
    public float criticalThreshold = 30f;
    public float emptyThreshold = 0f;
}

public class NeedsComp : AttributeModule
{
    [SerializeField] private List<NeedData> _needConfigs = new();
    private Dictionary<string, Need> _needs = new();

    protected override string ModuleName => "Needs";

    public IReadOnlyDictionary<string, Need> Needs => _needs;

    public override void Init()
    {
        base.Init();
        _needs.Clear();
        foreach (var config in _needConfigs)
        {
            var need = new Need
            {
                Name = config.name,
                MaxValue = config.maxValue,
                DecayRate = config.decayRate,
                CriticalThreshold = config.criticalThreshold,
                EmptyThreshold = config.emptyThreshold,
                CurrentValue = 0f
            };

            string name = config.name;
            need.OnCriticalThresholdReached += n => NotifyThresholdCrossed(name, n.CurrentValue, n.CriticalThreshold);
            need.OnCriticalThresholdLeft += n => NotifyThresholdCrossed(name, n.CurrentValue, n.CriticalThreshold);

            _needs[config.name] = need;
        }
    }

    public override void Update()
    {
        base.Update();
        float dt = Time.deltaTime;
        foreach (var kvp in _needs)
        {
            float old = kvp.Value.CurrentValue;
            kvp.Value.Update(dt);
            if (Math.Abs(kvp.Value.CurrentValue - old) > 0.001f)
            {
                NotifyAttributeChanged(kvp.Key, old, kvp.Value.CurrentValue);
            }
        }
    }

    public Need GetNeed(string name)
    {
        _needs.TryGetValue(name, out var need);
        return need;
    }

    public void SatisfyNeed(string name, float amount)
    {
        if (_needs.TryGetValue(name, out var need))
        {
            float old = need.CurrentValue;
            need.Satisfy(amount);
            NotifyAttributeChanged(name, old, need.CurrentValue);
        }
    }

    public bool IsNeedCritical(string name)
    {
        return _needs.TryGetValue(name, out var need) && need.IsCritical;
    }

    public override void LoadFromXml(XmlNode node)
    {
        _needConfigs.Clear();
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.Name != "Need") continue;

            var data = new NeedData
            {
                name = child.Attributes?["name"]?.Value ?? "",
                maxValue = float.TryParse(child.Attributes?["maxValue"]?.Value, out var max) ? max : 100f,
                decayRate = float.TryParse(child.Attributes?["decayRate"]?.Value, out var rate) ? rate : 0.5f,
                criticalThreshold = float.TryParse(child.Attributes?["criticalThreshold"]?.Value, out var crit) ? crit : 30f,
                emptyThreshold = float.TryParse(child.Attributes?["emptyThreshold"]?.Value, out var empty) ? empty : 0f
            };
            _needConfigs.Add(data);
        }
    }

    public string DebugString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"[NeedsComp] {_needs.Count} needs:");
        foreach (var kvp in _needs)
        {
            var n = kvp.Value;
            sb.AppendLine($"  {n.Name}: {n.CurrentValue:F1}/{n.MaxValue:F0} (rate={n.DecayRate:F2}/s, critical={n.IsCritical})");
        }
        return sb.ToString();
    }
}
