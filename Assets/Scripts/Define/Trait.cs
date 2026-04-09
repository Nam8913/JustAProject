using UnityEngine;

public sealed class Trait
{
    private string traitName;
    private string traitLabelName;
    private string description;

    enum TraitType
    {
        GoodTrait,
        BadTrait,
        NeutralTrait,
        Others
    }

    public Trait(string name, string labelName, string desc)
    {
        traitName = name;
        traitLabelName = labelName;
        description = desc;
    }

    public string Name
    {
        get => traitName;
    }

    public string LabelName
    {
        get => traitLabelName != null ? traitLabelName : traitName;
    }

    public string Description
    {
        get => description;
    }

    
}
