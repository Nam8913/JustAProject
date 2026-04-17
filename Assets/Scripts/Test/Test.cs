using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Test : MonoBehaviour
{
    World world;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        world = new World("testWorld", 12345);
        DebugNAVScripts debugScripts = this.gameObject.AddComponent<DebugNAVScripts>();

        Creature demoCreature = ThingHandler.CreateThing<Creature>();
        demoCreature.transform.position = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoSomeThing()
    {
        
    }
}
