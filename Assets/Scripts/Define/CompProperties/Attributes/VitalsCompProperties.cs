using System;

/// <summary>
/// CompProperties cho hệ thống chỉ số sống còn (Health, Stamina, Pain, Fatigue, etc.).
/// 
/// XML usage:
/// <code>
/// <compsProps>
///     <li Class="VitalsCompProperties">
///         <maxHealth>100</maxHealth>
///         <maxStamina>100</maxStamina>
///         <staminaRecoveryRate>5</staminaRecoveryRate>
///         <painDecayRate>2</painDecayRate>
///         <fatigueDecayRate>0.5</fatigueDecayRate>
///         <bleedingHealthLoss>2.0</bleedingHealthLoss>
///         <poisonHealthLoss>1.0</poisonHealthLoss>
///         <radiationHealthLoss>0.5</radiationHealthLoss>
///     </li>
/// </compsProps>
/// </code>
/// </summary>
[Serializable]
public class VitalsCompProperties : CompProperties
{
    /// <summary>
    /// Máu tối đa.
    /// </summary>
    public float maxHealth = 100f;

    /// <summary>
    /// Thể lực tối đa.
    /// </summary>
    public float maxStamina = 100f;

    /// <summary>
    /// Tốc độ hồi thể lực mỗi giây.
    /// </summary>
    public float staminaRecoveryRate = 5f;

    /// <summary>
    /// Tốc độ giảm đau mỗi giây.
    /// </summary>
    public float painDecayRate = 2f;

    /// <summary>
    /// Tốc độ giảm mệt mỏi mỗi giây.
    /// </summary>
    public float fatigueDecayRate = 0.5f;

    /// <summary>
    /// Máu mất mỗi giây khi chảy máu (per 100 bleeding).
    /// </summary>
    public float bleedingHealthLoss = 2f;

    /// <summary>
    /// Thể lực mất mỗi giây khi chảy máu.
    /// </summary>
    public float bleedingStaminaLoss = 0.5f;

    /// <summary>
    /// Máu mất mỗi giây khi bị ngộ độc (per 100 poison).
    /// </summary>
    public float poisonHealthLoss = 1f;

    /// <summary>
    /// Thể lực mất mỗi giây khi bị ngộ độc.
    /// </summary>
    public float poisonStaminaLoss = 0.5f;

    /// <summary>
    /// Máu mất mỗi giây khi bị nhiễm phóng xạ (per 100 radiation).
    /// </summary>
    public float radiationHealthLoss = 0.5f;
}
