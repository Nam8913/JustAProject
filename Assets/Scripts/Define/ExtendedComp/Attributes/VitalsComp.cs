using System;
using UnityEngine;

/// <summary>
/// Quản lý chỉ số sống còn của Creature (Health, Stamina, Pain, Fatigue, Bleeding, Poison, Radiation, Temperature).
/// Đọc cấu hình từ VitalsCompProperties.
/// </summary>
[Serializable]
public class VitalsComp : EntitiesComp
{
    // Runtime state
    private float _health;
    private float _stamina;
    private float _pain;
    private float _fatigue;
    private float _bleeding;
    private float _poison;
    private float _radiation;
    private float _temperature = 36.6f;

    // Threshold tracking
    private bool _wasPainCritical;
    private bool _wasFatigueCritical;

    // Cached properties
    private VitalsCompProperties _vitalsProps;

    // Events
    public event Action<string, float, float> OnVitalChanged;
    public event Action<string, float> OnVitalCritical;
    public event Action<string, float> OnVitalRecovered;
    public event Action OnDeath;

    // Public properties
    public float Health => _health;
    public float MaxHealth => _vitalsProps?.maxHealth ?? 100f;
    public float HealthNormalized => _health / MaxHealth;

    public float Stamina => _stamina;
    public float MaxStamina => _vitalsProps?.maxStamina ?? 100f;
    public float StaminaNormalized => _stamina / MaxStamina;

    public float Pain => _pain;
    public float Fatigue => _fatigue;
    public float Bleeding => _bleeding;
    public float Poison => _poison;
    public float Radiation => _radiation;
    public float Temperature => _temperature;

    // Multipliers
    public float ActionSpeedMultiplier => Mathf.Lerp(1f, 0.5f, _pain / 100f);
    public float MoveSpeedMultiplier => Mathf.Lerp(1f, 0.6f, _fatigue / 100f);
    public float AccuracyMultiplier => Mathf.Lerp(1f, 0.7f, _pain / 100f);
    public float StaminaRecoveryMultiplier => Mathf.Lerp(1f, 0.3f, _fatigue / 100f);

    public override void Init()
    {
        _vitalsProps = props as VitalsCompProperties;
        if (_vitalsProps == null)
        {
            return;
        }

        _health = _vitalsProps.maxHealth;
        _stamina = _vitalsProps.maxStamina;
    }

    public override void Update()
    {
        if (_vitalsProps == null)
        {
            return;
        }

        float dt = Time.deltaTime;

        UpdateStatusEffects(dt);
        UpdatePain(dt);
        UpdateFatigue(dt);
        UpdateStamina(dt);
        UpdateTemperature(dt);
    }

    private void UpdateStatusEffects(float dt)
    {
        // Bleeding
        if (_bleeding > 0)
        {
            float healthLoss = _vitalsProps.bleedingHealthLoss * (_bleeding / 100f) * dt;
            float staminaLoss = _vitalsProps.bleedingStaminaLoss * (_bleeding / 100f) * dt;

            ChangeHealth(-healthLoss);
            ChangeStamina(-staminaLoss);

            _bleeding = Mathf.Max(0, _bleeding - dt);
        }

        // Poison
        if (_poison > 0)
        {
            float healthLoss = _vitalsProps.poisonHealthLoss * (_poison / 100f) * dt;
            float staminaLoss = _vitalsProps.poisonStaminaLoss * (_poison / 100f) * dt;

            ChangeHealth(-healthLoss);
            ChangeStamina(-staminaLoss);

            _poison = Mathf.Max(0, _poison - dt * 0.5f);
        }

        // Radiation
        if (_radiation > 0)
        {
            float healthLoss = _vitalsProps.radiationHealthLoss * (_radiation / 100f) * dt;
            ChangeHealth(-healthLoss);

            _radiation = Mathf.Min(100f, _radiation + dt * 0.1f);
        }
    }

    private void UpdatePain(float dt)
    {
        if (_pain > 0)
        {
            float oldPain = _pain;
            _pain = Mathf.Max(0, _pain - _vitalsProps.painDecayRate * dt);

            // Kiểm tra crossing threshold
            bool isCritical = _pain >= 70;

            if (isCritical && !_wasPainCritical)
            {
                // Vượt ngưỡng critical (pain tăng trên 70)
                _wasPainCritical = true;
                OnVitalCritical?.Invoke("Pain", _pain);
            }
            else if (!isCritical && _wasPainCritical)
            {
                // Hồi phục từ critical (pain giảm dưới 70)
                _wasPainCritical = false;
                OnVitalRecovered?.Invoke("Pain", _pain);
            }
        }
    }

    private void UpdateFatigue(float dt)
    {
        if (_fatigue > 0)
        {
            float oldFatigue = _fatigue;
            _fatigue = Mathf.Max(0, _fatigue - _vitalsProps.fatigueDecayRate * dt);

            // Kiểm tra crossing threshold
            bool isCritical = _fatigue >= 80;

            if (isCritical && !_wasFatigueCritical)
            {
                // Vượt ngưỡng critical (fatigue tăng trên 80)
                _wasFatigueCritical = true;
                OnVitalCritical?.Invoke("Fatigue", _fatigue);
            }
            else if (!isCritical && _wasFatigueCritical)
            {
                // Hồi phục từ critical (fatigue giảm dưới 80)
                _wasFatigueCritical = false;
                OnVitalRecovered?.Invoke("Fatigue", _fatigue);
            }
        }
    }

    private void UpdateStamina(float dt)
    {
        if (_stamina < MaxStamina)
        {
            float recovery = _vitalsProps.staminaRecoveryRate * StaminaRecoveryMultiplier * dt;
            ChangeStamina(recovery);
        }
    }

    private void UpdateTemperature(float dt)
    {
        if (Mathf.Abs(_temperature - 36.6f) > 0.1f)
        {
            _temperature = Mathf.Lerp(_temperature, 36.6f, dt * 0.1f);
        }
    }

    // Public methods
    public void TakeDamage(float amount)
    {
        ChangeHealth(-amount);
    }

    public void Heal(float amount)
    {
        ChangeHealth(amount);
    }

    public void UseStamina(float amount)
    {
        ChangeStamina(-amount);
    }

    public void AddPain(float amount)
    {
        float oldPain = _pain;
        _pain = Mathf.Min(100f, _pain + amount);
        OnVitalChanged?.Invoke("Pain", oldPain, _pain);

        bool isCritical = _pain >= 70;
        if (isCritical && !_wasPainCritical)
        {
            _wasPainCritical = true;
            OnVitalCritical?.Invoke("Pain", _pain);
        }
    }

    public void AddFatigue(float amount)
    {
        float oldFatigue = _fatigue;
        _fatigue = Mathf.Min(100f, _fatigue + amount);
        OnVitalChanged?.Invoke("Fatigue", oldFatigue, _fatigue);

        bool isCritical = _fatigue >= 80;
        if (isCritical && !_wasFatigueCritical)
        {
            _wasFatigueCritical = true;
            OnVitalCritical?.Invoke("Fatigue", _fatigue);
        }
    }

    public void SetBleeding(float amount) => _bleeding = Mathf.Clamp(amount, 0, 100f);
    public void SetPoison(float amount) => _poison = Mathf.Clamp(amount, 0, 100f);
    public void SetRadiation(float amount) => _radiation = Mathf.Clamp(amount, 0, 100f);
    public void SetTemperature(float temp) => _temperature = Mathf.Clamp(temp, 30f, 45f);

    private void ChangeHealth(float delta)
    {
        float oldValue = _health;
        _health = Mathf.Clamp(_health + delta, 0, MaxHealth);

        if (Mathf.Abs(_health - oldValue) > 0.001f)
        {
            OnVitalChanged?.Invoke("Health", oldValue, _health);
        }

        if (_health <= 0 && oldValue > 0)
        {
            OnDeath?.Invoke();
        }
    }

    private void ChangeStamina(float delta)
    {
        float oldValue = _stamina;
        _stamina = Mathf.Clamp(_stamina + delta, 0, MaxStamina);

        if (Mathf.Abs(_stamina - oldValue) > 0.001f)
        {
            OnVitalChanged?.Invoke("Stamina", oldValue, _stamina);
        }
    }

    public string DebugString()
    {
        if (_vitalsProps == null)
        {
            return $"Vitals ({owner?.LabelName}): Not configured";
        }

        return $"Vitals ({owner?.LabelName}):\n" +
               $"  Health: {_health:F1}/{MaxHealth}\n" +
               $"  Stamina: {_stamina:F1}/{MaxStamina}\n" +
               $"  Pain: {_pain:F1}\n" +
               $"  Fatigue: {_fatigue:F1}\n" +
               $"  Bleeding: {_bleeding:F1}\n" +
               $"  Poison: {_poison:F1}\n" +
               $"  Radiation: {_radiation:F1}\n" +
               $"  Temperature: {_temperature:F1}°C";
    }
}
