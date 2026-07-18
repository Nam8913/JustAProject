using System;
using System.Xml;

/// <summary>
/// Base class for all attribute modules (Needs, Vitals, Attributes, Skills, Personality).
/// Provides common functionality for XML-configurable, observable attribute systems.
/// </summary>
public abstract class AttributeModule : EntitiesComp
{
    /// <summary>
    /// Called when any attribute value changes.
    /// Parameters: module name, attribute name, old value, new value
    /// </summary>
    public event Action<string, string, float, float> OnAttributeChanged;

    /// <summary>
    /// Called when an attribute crosses a threshold.
    /// Parameters: module name, attribute name, current value, threshold
    /// </summary>
    public event Action<string, string, float, float> OnThresholdCrossed;

    protected void NotifyAttributeChanged(string attributeName, float oldValue, float newValue)
    {
        OnAttributeChanged?.Invoke(ModuleName, attributeName, oldValue, newValue);
    }

    protected void NotifyThresholdCrossed(string attributeName, float currentValue, float threshold)
    {
        OnThresholdCrossed?.Invoke(ModuleName, attributeName, currentValue, threshold);
    }

    /// <summary>
    /// Module name for event identification
    /// </summary>
    protected abstract string ModuleName { get; }

    /// <summary>
    /// Parse XML configuration for this module
    /// </summary>
    public abstract void LoadFromXml(XmlNode node);
}
