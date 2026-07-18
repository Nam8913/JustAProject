using System.Xml;
using UnityEngine;

/// <summary>
/// Integration test MonoBehaviour that demonstrates NeedsComp, VitalsComp,
/// and AttributesComp working together. Attach to a GameObject in Play mode.
///
/// Keyboard controls:
///   H   - Take 10 damage
///   J   - Heal 5
///   K   - Use 20 stamina
///   L   - Add 30 pain
///   F1  - Show needs debug
///   F2  - Show vitals debug
///   F3  - Show attributes debug
/// </summary>
public class AttributesIntegrationTest : MonoBehaviour
{
    private NeedsComp _needs;
    private VitalsComp _vitals;
    private AttributesComp _attributes;

    private void Awake()
    {
        // EntitiesComp is not a MonoBehaviour, so we instantiate directly
        _needs = new NeedsComp();
        _vitals = new VitalsComp();
        _attributes = new AttributesComp();
    }

    private void Start()
    {
        // Load test XML for each module
        LoadNeedsXml();
        LoadVitalsXml();
        LoadAttributesXml();

        // Initialize all modules
        _needs.Init();
        _vitals.Init();
        _attributes.Init();

        // Subscribe to events
        _needs.OnAttributeChanged += (module, attr, oldVal, newVal) =>
            Debug.Log($"[Needs] {attr}: {oldVal:F1} -> {newVal:F1}");

        _needs.OnThresholdCrossed += (module, attr, current, threshold) =>
            Debug.LogWarning($"[Needs] THRESHOLD {attr}: {current:F1} (threshold={threshold:F1})");

        _vitals.OnAttributeChanged += (module, attr, oldVal, newVal) =>
            Debug.Log($"[Vitals] {attr}: {oldVal:F1} -> {newVal:F1}");

        _vitals.OnThresholdCrossed += (module, attr, current, threshold) =>
            Debug.LogWarning($"[Vitals] THRESHOLD {attr}: {current:F1} (threshold={threshold:F1})");

        _attributes.OnAttributeChanged += (module, attr, oldVal, newVal) =>
            Debug.Log($"[Attributes] {attr}: {oldVal:F1} -> {newVal:F1}");

        _attributes.OnThresholdCrossed += (module, attr, current, threshold) =>
            Debug.LogWarning($"[Attributes] THRESHOLD {attr}: {current:F1} (threshold={threshold:F1})");

        Debug.Log("[IntegrationTest] All modules initialized. Use H/J/K/L for actions, F1/F2/F3 for debug.");
    }

    private void Update()
    {
        _needs.Update();
        _vitals.Update();

        if (Input.GetKeyDown(KeyCode.H))
        {
            _vitals.TakeDamage(10f);
            Debug.Log($"[Action] Took 10 damage. Health={_vitals.Health:F1}");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            _vitals.Heal(5f);
            Debug.Log($"[Action] Healed 5. Health={_vitals.Health:F1}");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            _vitals.UseStamina(20f);
            Debug.Log($"[Action] Used 20 stamina. Stamina={_vitals.Stamina:F1}");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            _vitals.AddPain(30f);
            Debug.Log($"[Action] Added 30 pain. Pain={_vitals.Pain:F1}");
        }

        if (Input.GetKeyDown(KeyCode.F1))
            Debug.Log(_needs.DebugString());

        if (Input.GetKeyDown(KeyCode.F2))
            Debug.Log(_vitals.DebugString());

        if (Input.GetKeyDown(KeyCode.F3))
            Debug.Log(_attributes.DebugString());
    }

    private void LoadNeedsXml()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<Needs>
            <Need name=""Hunger"" maxValue=""100"" decayRate=""0.5"" criticalThreshold=""30"" />
            <Need name=""Thirst"" maxValue=""100"" decayRate=""0.8"" criticalThreshold=""25"" />
            <Need name=""Sleep"" maxValue=""100"" decayRate=""0.3"" criticalThreshold=""40"" />
        </Needs>");
        _needs.LoadFromXml(doc.DocumentElement);
    }

    private void LoadVitalsXml()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<Vitals>
            <health>100</health>
            <stamina>100</stamina>
            <pain>0</pain>
            <fatigue>0</fatigue>
            <bleeding>0</bleeding>
            <poison>0</poison>
            <radiation>0</radiation>
            <temperature>37</temperature>
        </Vitals>");
        _vitals.LoadFromXml(doc.DocumentElement);
    }

    private void LoadAttributesXml()
    {
        var doc = new XmlDocument();
        doc.LoadXml(@"<Attributes>
            <Attribute name=""Strength"">12</Attribute>
            <Attribute name=""Agility"">10</Attribute>
            <Attribute name=""Intelligence"">14</Attribute>
            <Attribute name=""Perception"">11</Attribute>
            <Attribute name=""Constitution"">13</Attribute>
            <Attribute name=""Charisma"">8</Attribute>
            <Attribute name=""Luck"">10</Attribute>
        </Attributes>");
        _attributes.LoadFromXml(doc.DocumentElement);
    }
}
