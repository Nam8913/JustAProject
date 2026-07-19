using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý nhu cầu sinh tồn của Creature (Hunger, Thirst, Sleep, Comfort, Hygiene).
/// Đọc cấu hình từ NeedsCompProperties.
/// 
/// Giá trị: 100 = đầy đủ, 0 = cực kỳ cần (giảm theo thời gian)
/// Khi giá trị dưới criticalThreshold → cảnh báo
/// </summary>
[Serializable]
public class NeedsComp : EntitiesComp
{
    /// <summary>
    /// Runtime state cho mỗi nhu cầu.
    /// </summary>
    private Dictionary<string, NeedState> _needs;

    /// <summary>
    /// Sự kiện khi giá trị nhu cầu thay đổi.
    /// Parameters: needName, oldValue, newValue
    /// </summary>
    public event Action<string, float, float> OnNeedChanged;

    /// <summary>
    /// Sự kiện khi nhu cầu đạt mức critical.
    /// Parameters: needName, currentValue
    /// </summary>
    public event Action<string, float> OnNeedCritical;

    /// <summary>
    /// Sự kiện khi nhu cầu hồi phục từ critical.
    /// Parameters: needName, currentValue
    /// </summary>
    public event Action<string, float> OnNeedRecovered;

    public override void Init()
    {
        _needs = new Dictionary<string, NeedState>();

        var needsProps = props as NeedsCompProperties;
        if (needsProps == null || needsProps.needs == null)
        {
            return;
        }

        foreach (var needData in needsProps.needs)
        {
            float startValue = needData.startValue >= 0 ? needData.startValue : needData.maxValue;

            _needs[needData.name] = new NeedState
            {
                Name = needData.name,
                CurrentValue = startValue,
                MaxValue = needData.maxValue,
                DecayRate = needData.decayRate,
                CriticalThreshold = needData.criticalThreshold,
                WasCritical = startValue < needData.criticalThreshold
            };
        }
    }

    public override void Update()
    {
        if (_needs == null)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        foreach (var kvp in _needs)
        {
            var need = kvp.Value;
            float oldValue = need.CurrentValue;

            // Giá trị GIẢM theo thời gian
            need.CurrentValue = Mathf.Max(0f, need.CurrentValue - need.DecayRate * deltaTime);

            // Thông báo thay đổi
            if (Mathf.Abs(need.CurrentValue - oldValue) > 0.001f)
            {
                OnNeedChanged?.Invoke(need.Name, oldValue, need.CurrentValue);
            }

            // Kiểm tra critical threshold
            bool isCritical = need.CurrentValue < need.CriticalThreshold;

            if (isCritical && !need.WasCritical)
            {
                need.WasCritical = true;
                OnNeedCritical?.Invoke(need.Name, need.CurrentValue);
            }
            else if (!isCritical && need.WasCritical)
            {
                need.WasCritical = false;
                OnNeedRecovered?.Invoke(need.Name, need.CurrentValue);
            }
        }
    }

    /// <summary>
    /// Lấy nhu cầu theo tên.
    /// </summary>
    public NeedState GetNeed(string name)
    {
        return _needs.TryGetValue(name, out var need) ? need : null;
    }

    /// <summary>
    /// Thỏa mãn nhu cầu (tăng giá trị).
    /// </summary>
    public void SatisfyNeed(string name, float amount)
    {
        var need = GetNeed(name);
        if (need != null)
        {
            float oldValue = need.CurrentValue;
            need.CurrentValue = Mathf.Min(need.MaxValue, need.CurrentValue + amount);
            OnNeedChanged?.Invoke(name, oldValue, need.CurrentValue);
        }
    }

    /// <summary>
    /// Kiểm tra nhu cầu có ở mức critical không.
    /// </summary>
    public bool IsNeedCritical(string name)
    {
        var need = GetNeed(name);
        return need != null && need.CurrentValue < need.CriticalThreshold;
    }

    /// <summary>
    /// Lấy tất cả nhu cầu (cho debug/UI).
    /// </summary>
    public IReadOnlyDictionary<string, NeedState> GetAllNeeds()
    {
        return _needs;
    }

    /// <summary>
    /// Lấy danh sách tên nhu cầu.
    /// </summary>
    public IEnumerable<string> GetNeedNames()
    {
        return (_needs?.Keys != null || _needs.Keys.Count > 0) ? _needs.Keys : Array.Empty<string>();
    }

    public string DebugString()
    {
        if (_needs == null || _needs.Count == 0)
        {
            return $"Needs ({owner?.LabelName}): No needs configured";
        }

        string result = $"Needs ({owner?.LabelName}):\n";
        foreach (var need in _needs.Values)
        {
            string status = need.IsCritical ? " [CRITICAL]" : "";
            result += $"  {need.Name}: {need.CurrentValue:F1}/{need.MaxValue}{status}\n";
        }
        return result;
    }

    /// <summary>
    /// Runtime state cho một nhu cầu.
    /// </summary>
    public class NeedState
    {
        public string Name;
        public float CurrentValue;
        public float MaxValue;
        public float DecayRate;
        public float CriticalThreshold;
        public bool WasCritical;

        public bool IsCritical => CurrentValue < CriticalThreshold;
        public bool IsEmpty => CurrentValue <= 0f;
        public float Normalized => CurrentValue / MaxValue;
    }
}
