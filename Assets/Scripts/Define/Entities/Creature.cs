#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using Unity.VisualScripting;
using UnityEngine;

public class Creature : DefineThing
{
    CircleCollider2D _collider;
    StatsHolder statsHolder;
    // BehaviorTree behaviorTree;

    public StatsHolder Stats => statsHolder;
    public Vector2 WorldPosition => transform.position;
    public Vector2Int CurrentChunkPosition => Chunk.GetChunkPosition(transform.position);

    public Creature()
    {
        statsHolder = new StatsHolder();
        BaseStats.AddAllBaseStatus(statsHolder);
        statsHolder.MS = 0.1f;
    }

    void Awake()
    {
        _collider = this.gameObject.AddComponent<CircleCollider2D>();

        // set default circle sprite for the creature
        SpriteRenderer sr = this.gameObject.GetOrAddComponent<SpriteRenderer>();
        sr.sprite = GlobalAssets.GetCircleSprite;
    }

    public override void Start()
    {
        base.Start();
    }

    public override void ConfigError()
    {
        base.ConfigError();
        #if DEBUG_LOG_FLAG && false
        Debug.Log(statsHolder.DebugString());
        Debug.Log(traitsHolder.DebugString());
        #endif
        // Additional error checks for Creature can be added here
    }
}
