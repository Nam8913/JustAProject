using System;

/// <summary>
/// Represents a single need (Hunger, Thirst, Sleep, etc.)
/// Value increases over time (0 = satisfied, 100 = critical need)
/// </summary>
[Serializable]
public class Need
{
    public string Name { get; set; }
    public float CurrentValue { get; set; }
    public float MaxValue { get; set; } = 100f;
    public float DecayRate { get; set; } = 0.5f; // per second
    public float CriticalThreshold { get; set; } = 30f;
    public float EmptyThreshold { get; set; } = 0f;

    /// <summary>
    /// Update need value based on time elapsed
    /// </summary>
    public void Update(float deltaTime)
    {
        float oldValue = CurrentValue;
        CurrentValue = Math.Min(MaxValue, CurrentValue + DecayRate * deltaTime);
        CurrentValue = Math.Max(0f, CurrentValue);

        // Check threshold crossings
        if (oldValue < CriticalThreshold && CurrentValue >= CriticalThreshold)
        {
            OnCriticalThresholdReached?.Invoke(this);
        }
        if (oldValue >= CriticalThreshold && CurrentValue < CriticalThreshold)
        {
            OnCriticalThresholdLeft?.Invoke(this);
        }
    }

    /// <summary>
    /// Satisfy need (reduce value)
    /// </summary>
    public void Satisfy(float amount)
    {
        CurrentValue = Math.Max(0f, CurrentValue - amount);
    }

    /// <summary>
    /// Is need at critical level?
    /// </summary>
    public bool IsCritical => CurrentValue >= CriticalThreshold;

    /// <summary>
    /// Is need completely empty?
    /// </summary>
    public bool IsEmpty => CurrentValue <= EmptyThreshold;

    /// <summary>
    /// Need as percentage (0-1)
    /// </summary>
    public float Normalized => CurrentValue / MaxValue;

    // Events
    public event Action<Need> OnCriticalThresholdReached;
    public event Action<Need> OnCriticalThresholdLeft;
}
