using System;
using System.Text;
using System.Xml;
using UnityEngine;

/// <summary>
/// Manages creature vital signs: Health, Stamina, Pain, Fatigue, Bleeding, Poison, Radiation, Temperature.
/// Processes status effects and exposes multiplier properties for action speed, move speed, and accuracy.
/// </summary>
[Serializable]
public class VitalsComp : AttributeModule
{
    // --- Vital values ---
    [SerializeField] private float _health = 100f;
    [SerializeField] private float _stamina = 100f;
    [SerializeField] private float _pain;
    [SerializeField] private float _fatigue;
    [SerializeField] private float _bleeding;
    [SerializeField] private float _poison;
    [SerializeField] private float _radiation;
    [SerializeField] private float _temperature = 37f;

    // --- Decay / recovery rates (per second) ---
    [SerializeField] private float _painDecayRate = 2f;
    [SerializeField] private float _fatigueDecayRate = 1f;
    [SerializeField] private float _staminaRecoveryRate = 5f;

    protected override string ModuleName => "Vitals";

    // --- Public read-only accessors ---
    public float Health => _health;
    public float Stamina => _stamina;
    public float Pain => _pain;
    public float Fatigue => _fatigue;
    public float Bleeding => _bleeding;
    public float Poison => _poison;
    public float Radiation => _radiation;
    public float Temperature => _temperature;

    public bool IsDead => _health <= 0f;
    public bool IsExhausted => _stamina <= 0f;

    // --- Multiplier properties ---
    // Pain: up to 50% action speed reduction, 30% accuracy reduction at pain=100
    public float ActionSpeedMultiplier => 1f - Mathf.Clamp01(_pain / 100f) * 0.5f;
    public float AccuracyMultiplier => 1f - Mathf.Clamp01(_pain / 100f) * 0.3f;

    // Fatigue: up to 40% move speed reduction, 70% stamina recovery reduction at fatigue=100
    public float MoveSpeedMultiplier => 1f - Mathf.Clamp01(_fatigue / 100f) * 0.4f;
    public float StaminaRecoveryMultiplier => 1f - Mathf.Clamp01(_fatigue / 100f) * 0.7f;

    // --- Public methods ---

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;
        float old = _health;
        _health = Mathf.Clamp(_health - amount, 0f, 100f);
        if (Mathf.Abs(_health - old) > 0.001f)
            NotifyAttributeChanged("Health", old, _health);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        float old = _health;
        _health = Mathf.Clamp(_health + amount, 0f, 100f);
        if (Mathf.Abs(_health - old) > 0.001f)
            NotifyAttributeChanged("Health", old, _health);
    }

    public void UseStamina(float amount)
    {
        if (amount <= 0f) return;
        float old = _stamina;
        _stamina = Mathf.Clamp(_stamina - amount, 0f, 100f);
        if (Mathf.Abs(_stamina - old) > 0.001f)
            NotifyAttributeChanged("Stamina", old, _stamina);
    }

    public void AddPain(float amount)
    {
        if (amount <= 0f) return;
        float old = _pain;
        _pain = Mathf.Clamp(_pain + amount, 0f, 100f);
        if (Mathf.Abs(_pain - old) > 0.001f)
            NotifyAttributeChanged("Pain", old, _pain);
    }

    public void AddFatigue(float amount)
    {
        if (amount <= 0f) return;
        float old = _fatigue;
        _fatigue = Mathf.Clamp(_fatigue + amount, 0f, 100f);
        if (Mathf.Abs(_fatigue - old) > 0.001f)
            NotifyAttributeChanged("Fatigue", old, _fatigue);
    }

    public void SetBleeding(float value)
    {
        float old = _bleeding;
        _bleeding = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Abs(_bleeding - old) > 0.001f)
            NotifyAttributeChanged("Bleeding", old, _bleeding);
    }

    public void SetPoison(float value)
    {
        float old = _poison;
        _poison = Mathf.Clamp(value, 0f, 100f);
        if (Mathf.Abs(_poison - old) > 0.001f)
            NotifyAttributeChanged("Poison", old, _poison);
    }

    public void AddRadiation(float amount)
    {
        if (amount <= 0f) return;
        float old = _radiation;
        _radiation = Mathf.Clamp(_radiation + amount, 0f, 100f);
        if (Mathf.Abs(_radiation - old) > 0.001f)
            NotifyAttributeChanged("Radiation", old, _radiation);
    }

    public void SetTemperature(float value)
    {
        float old = _temperature;
        _temperature = Mathf.Clamp(value, 30f, 45f);
        if (Mathf.Abs(_temperature - old) > 0.001f)
            NotifyAttributeChanged("Temperature", old, _temperature);
    }

    // --- Lifecycle ---

    public override void Init()
    {
        base.Init();
    }

    public override void Update()
    {
        base.Update();
        if (IsDead) return;

        float dt = Time.deltaTime;

        // --- Bleeding effect: -2.0 health/s, -0.5 stamina/s ---
        if (_bleeding > 0f)
        {
            ApplyChange("Health", ref _health, -2.0f * dt * (_bleeding / 100f));
            ApplyChange("Stamina", ref _stamina, -0.5f * dt * (_bleeding / 100f));
        }

        // --- Poison effect: -1.0 health/s, -0.5 stamina/s ---
        if (_poison > 0f)
        {
            ApplyChange("Health", ref _health, -1.0f * dt * (_poison / 100f));
            ApplyChange("Stamina", ref _stamina, -0.5f * dt * (_poison / 100f));
        }

        // --- Radiation effect: -0.5 health/s ---
        if (_radiation > 0f)
        {
            ApplyChange("Health", ref _health, -0.5f * dt * (_radiation / 100f));
        }

        // Check death after all damage sources
        if (_health <= 0f)
        {
            _health = 0f;
            return;
        }

        // --- Pain natural decay ---
        if (_pain > 0f)
        {
            ApplyChange("Pain", ref _pain, -_painDecayRate * dt);
        }

        // --- Fatigue natural decay ---
        if (_fatigue > 0f)
        {
            ApplyChange("Fatigue", ref _fatigue, -_fatigueDecayRate * dt);
        }

        // --- Stamina recovery (reduced by fatigue) ---
        if (_stamina < 100f)
        {
            float recovery = _staminaRecoveryRate * StaminaRecoveryMultiplier * dt;
            ApplyChange("Stamina", ref _stamina, recovery);
        }
    }

    // --- XML ---

    public override void LoadFromXml(XmlNode node)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            switch (child.Name)
            {
                case "health":
                    _health = ParseFloat(child, 100f);
                    break;
                case "stamina":
                    _stamina = ParseFloat(child, 100f);
                    break;
                case "pain":
                    _pain = ParseFloat(child, 0f);
                    break;
                case "fatigue":
                    _fatigue = ParseFloat(child, 0f);
                    break;
                case "bleeding":
                    _bleeding = ParseFloat(child, 0f);
                    break;
                case "poison":
                    _poison = ParseFloat(child, 0f);
                    break;
                case "radiation":
                    _radiation = ParseFloat(child, 0f);
                    break;
                case "temperature":
                    _temperature = ParseFloat(child, 37f);
                    break;
                case "painDecayRate":
                    _painDecayRate = ParseFloat(child, 2f);
                    break;
                case "fatigueDecayRate":
                    _fatigueDecayRate = ParseFloat(child, 1f);
                    break;
                case "staminaRecoveryRate":
                    _staminaRecoveryRate = ParseFloat(child, 5f);
                    break;
            }
        }
    }

    // --- Debug ---

    public string DebugString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("[VitalsComp]");
        sb.AppendLine($"  Health:    {_health:F1}/100");
        sb.AppendLine($"  Stamina:   {_stamina:F1}/100");
        sb.AppendLine($"  Pain:      {_pain:F1}/100");
        sb.AppendLine($"  Fatigue:   {_fatigue:F1}/100");
        sb.AppendLine($"  Bleeding:  {_bleeding:F1}");
        sb.AppendLine($"  Poison:    {_poison:F1}");
        sb.AppendLine($"  Radiation: {_radiation:F1}");
        sb.AppendLine($"  Temp:      {_temperature:F1}C");
        sb.AppendLine($"  Multipliers: ActionSpd={ActionSpeedMultiplier:F2} MoveSpd={MoveSpeedMultiplier:F2} Acc={AccuracyMultiplier:F2} StamRecov={StaminaRecoveryMultiplier:F2}");
        return sb.ToString();
    }

    // --- Helpers ---

    private void ApplyChange(string attrName, ref float field, float delta)
    {
        float old = field;
        field = Mathf.Clamp(field + delta, 0f, 100f);
        if (Mathf.Abs(field - old) > 0.001f)
            NotifyAttributeChanged(attrName, old, field);
    }

    private static float ParseFloat(XmlNode node, float defaultValue)
    {
        if (node == null) return defaultValue;
        string text = node.InnerText?.Trim();
        if (string.IsNullOrEmpty(text)) return defaultValue;
        return float.TryParse(text, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
    }
}
