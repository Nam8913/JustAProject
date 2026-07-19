using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BehaviorTree;
using BehaviorTree.Performance;
using BehaviorTree.Debug;

public class Test : MonoBehaviour
{
    private BehaviorTreeRunner _runner;
    private List<Blackboard> _blackboards = new List<Blackboard>();

    void Start()
    {
        GameService.Ins.GlobalInitialize();

        Creature player = ThingHandler.CreateThingById("HumanDef") as Creature;
        if (player != null)
        {
            GameService.Ins.SetFocusObject(player.gameObject);
        }

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

        //Spawn50NPCs();
        Spawn1NPC();
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
    private void Spawn1NPC()
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

        var creature = ThingHandler.CreateThingById("ZombieDef") as Creature;
        if (creature == null) return;
        creature.transform.position = (Vector3)(Random.insideUnitCircle * 30f);

        var bb = new Blackboard();
        bb.Set(BBKeys.HealthPercent, 1f);
        _blackboards.Add(bb);

        var (root, blackboard) = BuildSurvivalBehaviorTree(creature, bb);

        var runner = creature.gameObject.AddComponent<BehaviorTreeRunner>();
        runner.Initialize(root, blackboard);
        runner.enabled = false;

        var visionSensor = creature.gameObject.AddComponent<VisionSensor>();
        visionSensor.Initialize(blackboard);

        #if UNITY_EDITOR
        creature.gameObject.AddComponent<BTGizmos>();
        #endif
        scheduler.Register(runner);
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
            var creature = ThingHandler.CreateThingById("ZombieDef") as Creature;
            if (creature == null) continue;
            creature.transform.position = (Vector3)(Random.insideUnitCircle * 30f);

            var bb = new Blackboard();
            bb.Set(BBKeys.HealthPercent, 1f);
            _blackboards.Add(bb);

            var (root, blackboard) = BuildSurvivalBehaviorTree(creature, bb);

            var runner = creature.gameObject.AddComponent<BehaviorTreeRunner>();
            runner.Initialize(root, blackboard);
            runner.enabled = false;

            var visionSensor = creature.gameObject.AddComponent<VisionSensor>();
            visionSensor.Initialize(blackboard);

            #if UNITY_EDITOR
            creature.gameObject.AddComponent<BTGizmos>();
            #endif
            scheduler.Register(runner);
        }

        Debug.Log($"[Test] Spawned 50 NPCs. Scheduler managing {scheduler.RegisteredCount} NPCs.");
    }

    /// <summary>
    /// Xây dựng cây hành vi phức tạp cho NPC sinh tồn
    /// Độ sâu tối đa: 4 levels
    /// </summary>
    private (RootNode root, Blackboard blackboard) BuildSurvivalBehaviorTree(Creature creature, Blackboard bb)
    {
        return new BehaviorTreeBuilder(bb)
            .Root()
                .Selector("RootSelector")

                    // ═══════════════════════════════════════════
                    // COMBAT BRANCH - Ưu tiên cao nhất
                    // ═══════════════════════════════════════════
                    .Sequence("CombatBranch")
                        .Condition(new HasEnemyCondition())
                        .Selector("HandleThreat")
                            // Chiến thuật 1: Tìm cover rồi tấn công
                            // .Sequence("CoverAndAttack")
                            //     .Condition(new HasCoverNearbyCondition(creature, 10f))
                            //     .Action(new MoveToCoverAction(creature))
                            //     .Action(new AttackFromCoverAction(creature))
                            // .End()
                            // Chiến thuật 2: Tấn công trực tiếp
                            .Sequence("DirectAttack")
                                .Condition(new IsInAttackRangeCondition(creature, 3f))
                                .Action(new PerformAttackAction(creature))
                            .End()
                            // Chiến thuật 3: Tiếp cận kẻ thù
                            .Sequence("ApproachEnemy")
                                .Action(new MoveToEnemyAction(creature))
                            .End()
                        .End()
                    .End()

                    // ═══════════════════════════════════════════
                    // SURVIVAL BRANCH - Ưu tiên cao
                    // ═══════════════════════════════════════════
                    .Sequence("SurvivalBranch")
                        .Condition(new IsLowHealthCondition(0.3f))
                        .Selector("HandleHealing")
                            // Cách 1: Dùng item hồi máu
                            .Sequence("UseHealingItem")
                                .Condition(new HasHealingItemCondition())
                                .Action(new ConsumeHealItemAction())
                            .End()
                            // Cách 2: Tìm nơi an toàn
                            .Sequence("FindSafeSpot")
                                .Action(new FindSafeLocationAction(creature))
                                .Action(new MoveToTargetAction(creature))
                                .Action(new WaitAction(3f))
                            .End()
                        .End()
                    .End()

                    // ═══════════════════════════════════════════
                    // INVESTIGATE BRANCH - Ưu tiên trung bình
                    // ═══════════════════════════════════════════
                    .Sequence("InvestigateBranch")
                        .Condition(new HeardNoiseCondition())
                        .Sequence("InvestigateNoise")
                            .Action(new MoveToNoiseSourceAction(creature))
                            .Action(new LookAroundAction(creature))
                        .End()
                    .End()

                    // ═══════════════════════════════════════════
                    // GATHER BRANCH - Ưu tiên thấp
                    // ═══════════════════════════════════════════
                    .Sequence("GatherBranch")
                        .Condition(new IsHungryCondition())
                        .Sequence("FindFood")
                            .Action(new LocateFoodSourceAction(creature))
                            .Action(new MoveToTargetAction(creature))
                            .Action(new EatFoodAction())
                        .End()
                    .End()

                    // ═══════════════════════════════════════════
                    // WANDER BRANCH - Ưu tiên thấp nhất
                    // ═══════════════════════════════════════════
                    .Sequence("WanderBranch")
                        .Action(new PickRandomDestinationAction(creature, 15f))
                        .Action(new MoveToTargetAction(creature))
                        .Cooldown(2f)
                            .Action(new WaitAction(1f))
                        .End()
                    .End()

                .End()
            .BuildWithBlackboard();
    }
}

// ═══════════════════════════════════════════════════════════════════
// CONDITION NODES
// ═══════════════════════════════════════════════════════════════════

public class HasEnemyCondition : ConditionNode
{
    protected override bool Check()
    {
        return Blackboard.Has(BBKeys.CanSeeEnemy) && Blackboard.Has(BBKeys.ThreatTarget);
    }
}

public class HasCoverNearbyCondition : ConditionNode
{
    private readonly Creature _creature;
    private readonly float _coverRadius;

    public HasCoverNearbyCondition(Creature creature, float coverRadius)
    {
        _creature = creature;
        _coverRadius = coverRadius;
    }

    protected override bool Check()
    {
        // Tìm vật thể che chắn gần đó
        var colliders = Physics2D.OverlapCircleAll(_creature.WorldPosition, _coverRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Cover"))
            {
                Blackboard.Set(BBKeys.CoverPosition, (Vector3)col.transform.position);
                return true;
            }
        }
        return false;
    }
}

public class IsInAttackRangeCondition : ConditionNode
{
    private readonly Creature _creature;
    private readonly float _attackRange;

    public IsInAttackRangeCondition(Creature creature, float attackRange)
    {
        _creature = creature;
        _attackRange = attackRange;
    }

    protected override bool Check()
    {
        if (!Blackboard.TryGet(BBKeys.ThreatTarget, out DefineThing target))
            return false;

        float distance = Vector2.Distance(_creature.WorldPosition, target.transform.position);
        return distance <= _attackRange;
    }
}

public class IsLowHealthCondition : ConditionNode
{
    private readonly float _threshold;

    public IsLowHealthCondition(float threshold)
    {
        _threshold = threshold;
    }

    protected override bool Check()
    {
        if (Blackboard.TryGet(BBKeys.HealthPercent, out float health))
            return health < _threshold;
        return false;
    }
}

public class HasHealingItemCondition : ConditionNode
{
    protected override bool Check()
    {
        if (Blackboard.TryGet(BBKeys.HasHealingItem, out bool hasItem))
            return hasItem;
        return false;
    }
}

public class HeardNoiseCondition : ConditionNode
{
    protected override bool Check()
    {
        return Blackboard.Has(BBKeys.HeardNoise);
    }
}

public class IsHungryCondition : ConditionNode
{
    protected override bool Check()
    {
        if (Blackboard.TryGet(BBKeys.IsHungry, out bool isHungry))
            return isHungry;
        return false;
    }
}

// ═══════════════════════════════════════════════════════════════════
// HELPER - Xoay hướng theo hướng di chuyển
// ═══════════════════════════════════════════════════════════════════

public static class MovementHelper
{
    private const float RotationSpeed = 20f;

    /// <summary>
    /// Xoay transform.right theo hướng di chuyển
    /// </summary>
    public static void RotateTowardsMovement(Creature creature, Vector2 targetPos)
    {
        Vector2 currentPos = creature.WorldPosition;
        Vector2 direction = (targetPos - currentPos).normalized;

        // Chỉ xoay khi di chuyển
        if (direction.sqrMagnitude > 0.001f)
        {
            // Lerp mượt
            Vector2 newRight = Vector2.Lerp(creature.transform.right, direction, RotationSpeed * Time.deltaTime);
            creature.transform.right = newRight.normalized;
        }
    }

    /// <summary>
    /// Xoay transform.right theo hướng cụ thể (instant)
    /// </summary>
    public static void LookAt(Creature creature, Vector2 targetPos)
    {
        Vector2 direction = (targetPos - creature.WorldPosition).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            creature.transform.right = direction;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION NODES - Combat
// ═══════════════════════════════════════════════════════════════════

public class MoveToCoverAction : ActionNode
{
    private readonly Creature _creature;
    private float _moveSpeed;

    public MoveToCoverAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _moveSpeed = _creature.Stats.MS * 10f;
    }

    protected override BHState OnEvaluate()
    {
        if (!Blackboard.TryGet(BBKeys.CoverPosition, out Vector3 coverPos))
            return BHState.Failure;

        float distance = Vector2.Distance(_creature.WorldPosition, coverPos);
        return distance < 0.5f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        if (!Blackboard.TryGet(BBKeys.CoverPosition, out Vector3 coverPos))
            return BHState.Failure;

        // Xoay hướng theo hướng di chuyển
        MovementHelper.RotateTowardsMovement(_creature, coverPos);

        _creature.transform.position = Vector2.MoveTowards(
            _creature.WorldPosition, coverPos, _moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(_creature.WorldPosition, coverPos);
        return distance < 0.5f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class AttackFromCoverAction : ActionNode
{
    private readonly Creature _creature;
    private float _attackDuration = 1.5f;
    private float _elapsed;

    public AttackFromCoverAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _elapsed = 0f;
        Debug.Log($"[{_creature.name}] Attacking from cover!");
    }

    protected override BHState OnEvaluate()
    {
        _elapsed += Time.deltaTime;
        return _elapsed >= _attackDuration ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        // Animation attack, spawn projectile, etc.
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class PerformAttackAction : ActionNode
{
    private readonly Creature _creature;
    private float _attackDuration = 1f;
    private float _elapsed;

    public PerformAttackAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _elapsed = 0f;
        Debug.Log($"[{_creature.name}] Direct attack!");
    }

    protected override BHState OnEvaluate()
    {
        _elapsed += Time.deltaTime;
        return _elapsed >= _attackDuration ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        // Animation attack, deal damage, etc.
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class MoveToEnemyAction : ActionNode
{
    private readonly Creature _creature;
    private float _moveSpeed;

    public MoveToEnemyAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _moveSpeed = _creature.Stats.MS * 10f;
    }

    protected override BHState OnEvaluate()
    {
        if (!Blackboard.TryGet(BBKeys.ThreatTarget, out DefineThing target))
            return BHState.Failure;

        float distance = Vector2.Distance(_creature.WorldPosition, target.transform.position);
        return distance <= 2f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        if (!Blackboard.TryGet(BBKeys.ThreatTarget, out DefineThing target))
            return BHState.Failure;

        // Xoay hướng theo hướng di chuyển
        MovementHelper.RotateTowardsMovement(_creature, target.transform.position);

        _creature.transform.position = Vector2.MoveTowards(
            _creature.WorldPosition, target.transform.position, _moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(_creature.WorldPosition, target.transform.position);
        return distance <= 2f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION NODES - Survival
// ═══════════════════════════════════════════════════════════════════

public class ConsumeHealItemAction : ActionNode
{
    protected override void OnEnter()
    {
        Debug.Log("[BT] Consuming healing item...");
    }

    protected override BHState OnEvaluate()
    {
        // Heal instantly
        if (Blackboard.TryGet(BBKeys.HealthPercent, out float health))
        {
            health = Mathf.Min(1f, health + 0.5f);
            Blackboard.Set(BBKeys.HealthPercent, health);
            Blackboard.Set(BBKeys.HasHealingItem, false);
        }
        return BHState.Success;
    }

    protected override BHState OnExecute()
    {
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class FindSafeLocationAction : ActionNode
{
    private readonly Creature _creature;

    public FindSafeLocationAction(Creature creature) { _creature = creature; }

    protected override BHState OnEvaluate()
    {
        // Tìm vị trí xa kẻ thù
        Vector2 safePos = _creature.WorldPosition + Random.insideUnitCircle * 20f;
        Blackboard.Set(BBKeys.MoveTarget, (Vector3)safePos);
        return BHState.Success;
    }

    protected override BHState OnExecute()
    {
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION NODES - Investigate
// ═══════════════════════════════════════════════════════════════════

public class MoveToNoiseSourceAction : ActionNode
{
    private readonly Creature _creature;
    private float _moveSpeed;

    public MoveToNoiseSourceAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _moveSpeed = _creature.Stats.MS * 10f;
    }

    protected override BHState OnEvaluate()
    {
        if (!Blackboard.TryGet(BBKeys.NoisePosition, out Vector3 noisePos))
            return BHState.Failure;

        float distance = Vector2.Distance(_creature.WorldPosition, noisePos);
        return distance < 1f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        if (!Blackboard.TryGet(BBKeys.NoisePosition, out Vector3 noisePos))
            return BHState.Failure;

        // Xoay hướng theo hướng di chuyển
        MovementHelper.RotateTowardsMovement(_creature, noisePos);

        _creature.transform.position = Vector2.MoveTowards(
            _creature.WorldPosition, noisePos, _moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(_creature.WorldPosition, noisePos);
        return distance < 1f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class LookAroundAction : ActionNode
{
    private readonly Creature _creature;
    private float _lookDuration = 2f;
    private float _elapsed;
    private float _startAngle;

    public LookAroundAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _elapsed = 0f;
        _startAngle = Mathf.Atan2(_creature.transform.right.y, _creature.transform.right.x) * Mathf.Rad2Deg;
        Debug.Log($"[{_creature.name}] Looking around...");
        Blackboard.Remove(BBKeys.HeardNoise);
    }

    protected override BHState OnEvaluate()
    {
        _elapsed += Time.deltaTime;
        return _elapsed >= _lookDuration ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        // Quay đầu nhìn xung quanh (quay 360 độ trong thời gian _lookDuration)
        float t = _elapsed / _lookDuration;
        float angle = _startAngle + t * 360f;
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        _creature.transform.right = dir;

        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION NODES - Gather
// ═══════════════════════════════════════════════════════════════════

public class LocateFoodSourceAction : ActionNode
{
    private readonly Creature _creature;

    public LocateFoodSourceAction(Creature creature) { _creature = creature; }

    protected override BHState OnEvaluate()
    {
        // Tìm nguồn thức ăn gần nhất
        var foodSources = GameObject.FindGameObjectsWithTag("Food");
        if (foodSources.Length == 0)
            return BHState.Failure;

        GameObject closest = null;
        float closestDist = float.MaxValue;

        foreach (var food in foodSources)
        {
            float dist = Vector2.Distance(_creature.WorldPosition, food.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = food;
            }
        }

        if (closest != null)
        {
            Blackboard.Set(BBKeys.MoveTarget, closest.transform.position);
            Blackboard.Set(BBKeys.FoodTarget, closest.GetComponent<DefineThing>());
            return BHState.Success;
        }

        return BHState.Failure;
    }

    protected override BHState OnExecute()
    {
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class EatFoodAction : ActionNode
{
    private float _eatDuration = 2f;
    private float _elapsed;

    protected override void OnEnter()
    {
        _elapsed = 0f;
        Debug.Log("[BT] Eating food...");
    }

    protected override BHState OnEvaluate()
    {
        _elapsed += Time.deltaTime;

        if (_elapsed >= _eatDuration)
        {
            // Hồi phục khi ăn
            if (Blackboard.TryGet(BBKeys.HealthPercent, out float health))
            {
                health = Mathf.Min(1f, health + 0.2f);
                Blackboard.Set(BBKeys.HealthPercent, health);
            }
            Blackboard.Set(BBKeys.IsHungry, false);
            return BHState.Success;
        }

        return BHState.Running;
    }

    protected override BHState OnExecute()
    {
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION NODES - Wander
// ═══════════════════════════════════════════════════════════════════

public class PickRandomDestinationAction : ActionNode
{
    private readonly Creature _creature;
    private readonly float _radius;

    public PickRandomDestinationAction(Creature creature, float radius)
    {
        _creature = creature;
        _radius = radius;
    }

    protected override BHState OnEvaluate()
    {
        Vector2 randomPos = _creature.WorldPosition + Random.insideUnitCircle * _radius;
        Blackboard.Set(BBKeys.MoveTarget, (Vector3)randomPos);
        return BHState.Success;
    }

    protected override BHState OnExecute()
    {
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class MoveToTargetAction : ActionNode
{
    private readonly Creature _creature;
    private float _moveSpeed;

    public MoveToTargetAction(Creature creature) { _creature = creature; }

    protected override void OnEnter()
    {
        _moveSpeed = _creature.Stats.MS * 10f;
    }

    protected override BHState OnEvaluate()
    {
        if (!Blackboard.TryGet(BBKeys.MoveTarget, out Vector3 target))
            return BHState.Failure;

        float distance = Vector2.Distance(_creature.WorldPosition, target);
        return distance < 0.3f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        if (!Blackboard.TryGet(BBKeys.MoveTarget, out Vector3 target))
            return BHState.Failure;

        // Xoay hướng theo hướng di chuyển
        MovementHelper.RotateTowardsMovement(_creature, target);

        _creature.transform.position = Vector2.MoveTowards(
            _creature.WorldPosition, target, _moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(_creature.WorldPosition, target);
        return distance < 0.3f ? BHState.Success : BHState.Running;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}

public class WaitAction : ActionNode
{
    private readonly float _waitDuration;
    private float _elapsed;

    public WaitAction(float duration)
    {
        _waitDuration = duration;
    }

    protected override void OnEnter()
    {
        _elapsed = 0f;
    }

    protected override BHState OnEvaluate()
    {
        _elapsed += Time.deltaTime;
        return _elapsed >= _waitDuration ? BHState.Success : BHState.Running;
    }

    protected override BHState OnExecute()
    {
        return EvaluatedState;
    }

    protected override BHState OnUpdate() { OnEvaluate(); return OnExecute(); }
}
