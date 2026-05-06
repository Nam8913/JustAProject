using UnityEngine;

public class Game_Play : Game
{
    World world;
    public override void Start()
    {
        base.Start();
        world = GameService.GetWorld();
        if(world != null)
        {
            GameService.SetNoise(new ModernHashNoise(world.Seed));
            GameService.SetWorldHandler(new GameObject("WorldHandler").AddComponent<WorldHandler>());
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
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
