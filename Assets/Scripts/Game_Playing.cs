using UnityEngine;
using UnityEngine.InputSystem;

public class Game_Play : Game
{
    World world;
    public override void Start()
    {
        base.Start();
        #if UNITY_EDITOR
        world = new World("TestWorld", 12345);
        #else
        world = GameService.GetWorld();
        #endif
        
        if(world != null)
        {
            GameService.SetNoise(new ModernHashNoise(world.Seed));
            GameService.SetWorldHandler(new GameObject("WorldHandler").AddComponent<WorldHandler>());
        }

         

        DefineThing player = ThingHandler.CreateThingById("HumanDef");

        GameObject sprite = new GameObject("PlayerSprite");
        sprite.transform.position = new Vector3(0, 0, -1);
        sprite.transform.localScale = new Vector3(0.3f, 0.3f, 1);
        sprite.transform.SetParent(player.transform);

        SpriteRenderer spriteRenderer = sprite.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = LocalRefDefaultRS.GetSpriteByName("Circle");

        GameService.Ins.SetFocusObject(player.gameObject);
        //ShowInventoryGUI.Instance.SetTargetToShow(player);
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Vector2 playerPos = GameService.Ins.GetFocusObject().transform.position;
            Tile tile = GameService.GetWorldHandler().GetChunk(playerPos).GetTileAtWorldPosition(playerPos);
            tile.GroundContainer.TryAddItem("log",1); 
            Debug.Log("Added log to tile at (0,0)");
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
