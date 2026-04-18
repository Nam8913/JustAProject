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
        SpriteRenderer renderer = demoCreature.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = LocalRefDefaultRS.GetSpriteByName("Circle");

        CircleCollider2D collider2D = demoCreature.gameObject.AddComponent<CircleCollider2D>();


        demoCreature.transform.position = Vector2.zero;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (PlayerInput.isMouseWasPressedThisFrame(0))
        // {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     RaycastHit hit;

        //     if (Physics.Raycast(ray, out hit))
        //     {
        //     Debug.Log("Hit: " + hit.collider.name);
        //     }
        // }

        Debug.DrawRay((Vector3)PlayerInput.MousePosition + new Vector3(0,0,0.9f), new Vector3(0,0,-1), Color.red, 0.1f);

        if(PlayerInput.isButtonPressed("Jump"))
        {
            Debug.Log("Jump button was pressed this frame.");
        }

        if(PlayerInput.isButtonDown("Jump"))
        {
            Debug.Log("Jump button is being held down.");
        }

        if(PlayerInput.isButtonReleased("Jump"))
        {
            Debug.Log("Jump button was released this frame.");
        }
    }

    public void DoSomeThing()
    {
        
    }

    void OnDrawGizmos()
    {
        
    }
}
