using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehaviorTree;
using BehaviorTree.Performance;
using BehaviorTree.Debug;

public class Test : MonoBehaviour
{
    private BehaviorTreeRunner _runner;
    private List<Creature> _creatures = new List<Creature>();
    private List<Blackboard> _blackboards = new List<Blackboard>();

    void Start()
    {
        GameService.Ins.GlobalInitialize();
        GameService.PlayerInput.Enable();

        // Add debug components
        if (FindAnyObjectByType<BTLogger>() == null)
        {
            var loggerGO = new GameObject("BTLogger");
            loggerGO.AddComponent<BTLogger>();
        }

        if (FindAnyObjectByType<BTStats>() == null)
        {
            var statsGO = new GameObject("BTStats");
            statsGO.AddComponent<BTStats>();
        }

        Spawn50NPCs();
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
    }

    void OnGUI()
    {
    }

    void OnDrawGizmos()
    {
        if (_creatures == null) return;

        foreach (var creature in _creatures)
        {
            if (creature == null) continue;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(creature.WorldPosition, 0.1f);
        }
    }

    [MakeButtonFuncOnTestClass(true)]
    void DoSomeThing()
    {
        // Test: Set enemy cho tất cả creatures
        foreach (var bb in _blackboards)
        {
            bb.Set(BBKeys.InCombat, true);
        }
        Debug.Log("[Test] Set InCombat = true for all NPCs");
    }

    [MakeButtonFuncOnTestClass(true)]
    void DoSomeThingElse()
    {
        // Test: Xóa enemy cho tất cả creatures
        foreach (var bb in _blackboards)
        {
            bb.Remove(BBKeys.InCombat);
        }
        Debug.Log("[Test] Removed InCombat for all NPCs");
    }

    [MakeButtonFuncOnTestClass(true)]
    void Spawn50NPCs()
    {
        var scheduler = FindAnyObjectByType<BTScheduler>();
        if (scheduler == null)
        {
            var schedulerGO = new GameObject("BTScheduler");
            scheduler = schedulerGO.AddComponent<BTScheduler>();
        }
        var mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[Test] No MainCamera found! Add a camera tagged 'MainCamera'.");
            return;
        }
        scheduler.SetPlayerTransform(mainCam.transform);

        for (int i = 0; i < 50; i++)
        {
            var creature = ThingHandler.CreateThingById("HumanDef") as Creature;
            if (creature == null) continue;
            _creatures.Add(creature);
            // Random position around origin
            creature.transform.position = (Vector3)(Random.insideUnitCircle * 30f);

            var bb = new Blackboard();
            bb.Set(BBKeys.HealthPercent, 1f);
            _blackboards.Add(bb);

            var (root, blackboard) = new BehaviorTreeBuilder(bb)
                .Root()
                    .Selector()
                        .Sequence()
                            .Condition(new HasEnemyCondition())
                            .Action(new SimpleAttackAction())
                        .End()
                        .Sequence()
                            .Action(new SimpleAcquireTargetAction(creature, 8f))
                            .Action(new SimpleMoveAction(creature))
                        .End()
                    .End()
                .BuildWithBlackboard();

            var runner = creature.gameObject.AddComponent<BehaviorTreeRunner>();
            runner.Initialize(root, blackboard);
            runner.enabled = false; // Scheduler handles ticking

            // Add gizmos for scene view debugging
            creature.gameObject.AddComponent<BTGizmos>();

            scheduler.Register(runner);
        }

        Debug.Log($"[Test] Spawned 50 NPCs. Scheduler managing {scheduler.RegisteredCount} NPCs.");
    }
}

// === Condition Nodes ===

public class HasEnemyCondition : ConditionNode
{
    protected override bool Check()
    {
        return Blackboard.Has(BBKeys.InCombat);
    }
}

// === Action Nodes (Two-Phase: Evaluate = logic, Execute = Unity API) ===

public class SimpleAcquireTargetAction : ActionNode
{
    private readonly Creature _creature;
    private readonly float _wanderRadius;

    public SimpleAcquireTargetAction(Creature creature, float wanderRadius)
    {
        _creature = creature;
        _wanderRadius = wanderRadius;
    }

    // Phase 1: Pure logic (thread-safe) - decide target
    protected override BehaviorTree.BHState OnEvaluate()
    {
        Vector2 currentPos = _creature.WorldPosition;
        Vector2 randomOffset = Random.insideUnitCircle * _wanderRadius;
        Vector2 targetPos = currentPos + randomOffset;

        // Store in blackboard for Execute phase
        Blackboard.Set(BBKeys.MoveTarget, (Vector3)targetPos);

        return BehaviorTree.BHState.Success;
    }

    // Phase 2: No Unity API needed
    protected override BehaviorTree.BHState OnExecute()
    {
        return EvaluatedState;
    }

    // Legacy: full tick on main thread
    protected override BehaviorTree.BHState OnUpdate()
    {
        OnEvaluate();
        return OnExecute();
    }

}

public class SimpleMoveAction : ActionNode
{
    private readonly Creature _creature;
    private float _moveSpeed = 2f;

    public SimpleMoveAction(Creature creature)
    {
        _creature = creature;
    }

    protected override void OnEnter()
    {
        _moveSpeed = _creature.Stats.MS * 10f;
    }

    // Phase 1: Pure logic (thread-safe) - check if we can move
    protected override BehaviorTree.BHState OnEvaluate()
    {
        if (!Blackboard.TryGet(BBKeys.MoveTarget, out Vector3 target))
            return BehaviorTree.BHState.Failure;

        float distance = Vector3.Distance(_creature.transform.position, target);
        if (distance < 0.1f)
            return BehaviorTree.BHState.Success;

        return BehaviorTree.BHState.Running;
    }

    // Phase 2: Unity API (main thread) - actually move
    protected override BehaviorTree.BHState OnExecute()
    {
        if (!Blackboard.TryGet(BBKeys.MoveTarget, out Vector3 target))
            return BehaviorTree.BHState.Failure;

        Vector3 currentPos = _creature.transform.position;
        float distance = Vector3.Distance(currentPos, target);

        if (distance < 0.1f)
            return BehaviorTree.BHState.Success;

        // Unity API: Transform.position
        _creature.transform.position = Vector3.MoveTowards(
            currentPos,
            target,
            _moveSpeed * Time.deltaTime
        );

        return BehaviorTree.BHState.Running;
    }

    // Legacy: full tick on main thread
    protected override BehaviorTree.BHState OnUpdate()
    {
        OnEvaluate();
        return OnExecute();
    }

}

public class SimpleAttackAction : ActionNode
{
    private float _attackDuration = 1f;
    private float _elapsed;

    protected override void OnEnter()
    {
        _elapsed = 0f;
    }

    // Phase 1: Pure logic (thread-safe) - check timer
    protected override BehaviorTree.BHState OnEvaluate()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= _attackDuration)
            return BehaviorTree.BHState.Success;

        return BehaviorTree.BHState.Running;
    }

    // Phase 2: Unity API (main thread) - attack effects
    protected override BehaviorTree.BHState OnExecute()
    {
        // Here you would play animation, spawn particles, etc.
        // For now just return the evaluated state
        return EvaluatedState;
    }

    // Legacy: full tick on main thread
    protected override BehaviorTree.BHState OnUpdate()
    {
        OnEvaluate();
        return OnExecute();
    }

}
