using UnityEngine;

public sealed class Status
{
    private string statusName;
    private string description;
    private float value;

    public Status(string name, string desc, float value = 0f)
    {
        statusName = name;
        description = desc;
        this.value = value;
    }

    public string Name
    {
        get => statusName;
    }

    public string Description
    {
        get => description;
    }

    public float Value
    {
        get => value;
        set
        {
            this.value = value;
        }
    }

    
}
