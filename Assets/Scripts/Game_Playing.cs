using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game_Play : Game
{
    World world;
    HandlePlayerInput playerInputHandler;
    BuildGhostController ghostController;

    [Header("Build Mode Settings")]
    [SerializeField] private GameObject structurePrefab; // Assign in inspector or will use placeholder

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

        CreatePlayingHolder();
        // Initialize structure pool for build mode
        InitializeStructurePool();

        // Setup build mode handlers
        ghostController = this.gameObject.AddComponent<BuildGhostController>();
        playerInputHandler = this.gameObject.AddComponent<HandlePlayerInput>();
    }

    private void CreatePlayingHolder()
    {
       HolderManager.CreateNewHolderObject(HolderManager.StructureHolderName);
    }

    private void InitializeStructurePool()
    {
        GameObject prefab = structurePrefab;

        // If no prefab assigned, create a simple placeholder at runtime
        if (prefab == null)
        {
            prefab = new GameObject("StructurePlaceholder", typeof(SpriteRenderer));
            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            
            // Try to load a fallback sprite from Resources
            Sprite fallback = LocalRefDefaultRS.GetSpriteByName("Square");
            if (fallback != null)
            {
                renderer.sprite = fallback;
            }
            else
            {
                // Create a simple white square texture at runtime
                Texture2D tex = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                renderer.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
            }

            prefab.SetActive(false);
            prefab.transform.position = Vector3.zero;
        }

        // Initialize the object pool with 50 pre-allocated objects
        PoolManager.CreateNewPool("Structure", prefab, 50);
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