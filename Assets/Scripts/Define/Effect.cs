using UnityEngine;

public class Effect
{
    private string effectName;
    private string effectLabelName;
    private string description;
    private float baseDurationValue;
    public Effect(string name, string labelName, string desc, float duration)
    {
        effectName = name;
        effectLabelName = labelName;
        description = desc;
        baseDurationValue = duration;
    }

    public string Name => effectName;
    public string LabelName => effectLabelName;
    public string Description => description;
    public float BaseDurationValue => baseDurationValue;
}
