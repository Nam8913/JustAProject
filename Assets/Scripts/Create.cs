using System.Collections.Generic;
using UnityEngine;

public static class Create
{
    static List<DefineThing> things = new List<DefineThing>();

    public static T CreateThing<T>() where T : DefineThing, new()
    {
        GameObject newObj = new GameObject(typeof(T).Name);
        T newThing = newObj.AddComponent<T>();
        newThing.ConfigError();
        things.Add(newThing);
        return newThing;
    }
}
