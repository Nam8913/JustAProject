using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class MakeButtonFuncOnTestClassAttribute : Attribute
{
    public bool isWorkOnlyInRuntime;
    public MakeButtonFuncOnTestClassAttribute(bool isWorkOnlyInRuntime = true)
    {
        this.isWorkOnlyInRuntime = isWorkOnlyInRuntime;
    }
}
