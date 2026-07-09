#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;

public class Creature : DefineThing
{
    StatsHolder statsHolder;
    TraitsHolder traitsHolder;
    BehaviorTree behaviorTree;

    public StatsHolder Stats => statsHolder;
    public TraitsHolder Traits => traitsHolder;
    
    public Creature()
    {
        labelName = "DefaultCreature";
        labelDescription = "A creature with no special traits and no special abilities just for test.";
        statsHolder = new StatsHolder();
        BaseStats.AddAllBaseStatus(statsHolder);
        statsHolder.MS = 0.1f;
        traitsHolder = new TraitsHolder();
    }

    void Awake()
    {
        // behaviorTree = GetComponent<BehaviorTree>();
        // if (behaviorTree == null)
        // {
        //     behaviorTree = gameObject.AddComponent<BehaviorTree>();
        // }

        // RootNodeBH rootNode = behaviorTree.rootNode;
        // if (rootNode == null)
        // {
        //     rootNode = BehaviorTreeFactory.CreateDefaultCreatureTree(this);
        // }

        // behaviorTree.Initialize(rootNode, this);
    }

    public override void Start()
    {
        base.Start();
    }

    public Vector2 WorldPosition => transform.position;

    public Vector2Int CurrentChunkPosition => Chunk.GetChunkPosition(transform.position);

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
