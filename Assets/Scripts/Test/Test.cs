using UnityEngine;

public class Test : MonoBehaviour
{
    // World world;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameService.Ins.GlobalInitialize();
        // world = new World("testWorld", 12345);
        // DebugNAVScripts debugScripts = this.gameObject.AddComponent<DebugNAVScripts>();

        // Creature demoCreature = ThingHandler.CreateThing<Creature>();
        // SpriteRenderer renderer = demoCreature.gameObject.AddComponent<SpriteRenderer>();
        // renderer.sprite = LocalRefDefaultRS.GetSpriteByName("Circle");

        // CircleCollider2D collider2D = demoCreature.gameObject.AddComponent<CircleCollider2D>();


        // demoCreature.transform.position = Vector2.zero;

    }

    // Update is called once per frame
    void Update()
    {
        

    }

    void FixedUpdate()
    {
        Vector2 move = PlayerInput.Move;
        if(move != Vector2.zero)
        {
            Camera.main.transform.position += new Vector3(move.x, move.y, 0) * Time.deltaTime * 5f;
        }
    }

    void OnGUI()
    {
        
    }

    [MakeButtonFuncOnTestClass]
    void DoSomeThing()
    {
        for(int i = 0; i <= 5; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(Vector2.zero + new Vector2(0.2f * i, 0.2f * i), -Vector2.up);
            if(hit)
            {
                Debug.Log($"Hit: {hit.collider.name} at position {hit.point}");
            }
        }
    }

    [MakeButtonFuncOnTestClass]
    void DoSomeThingElse()
    {
        GameObject square = new GameObject("Square1");
        SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
        renderer.sprite = LocalRefDefaultRS.GetSpriteByName("Square");
        square.transform.position = Vector2.zero + new Vector2(0.5f,0.5f);
        square.AddComponent<BoxCollider2D>();

         
    }
}
