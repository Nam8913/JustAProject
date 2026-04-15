using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree : MonoBehaviour
{
    public RootNodeBH rootNode;
    public BHState currentState = BHState.RUNNING;

    [SerializeField] private float tickInterval = 0f;
    private float timeSinceLastTick = 0f;

    public BehaviorTreeContext Context { get; private set; }
    public bool IsInitialized { get; private set; }

    void Awake()
    {
        if (Context == null)
        {
            Context = new BehaviorTreeContext(this);
        }
    }

    void Start()
    {
        if (!IsInitialized && rootNode != null)
        {
            Initialize(rootNode, GetComponent<Creature>());
        }
    }

    void Update()
    {
        if (!IsInitialized || rootNode == null)
        {
            return;
        }

        if (tickInterval <= 0f)
        {
            TickTree(Time.deltaTime);
            return;
        }

        timeSinceLastTick += Time.deltaTime;
        if (timeSinceLastTick < tickInterval)
        {
            return;
        }

        float deltaTime = timeSinceLastTick;
        timeSinceLastTick = 0f;
        TickTree(deltaTime);
    }

    public void Initialize(RootNodeBH newRootNode, Creature creature = null)
    {
        rootNode?.Reset();
        rootNode = newRootNode;

        if (Context == null)
        {
            Context = new BehaviorTreeContext(this, creature);
        }
        else
        {
            Context.AttachCreature(creature != null ? creature : Context.Creature);
        }

        if (Context.Creature == null)
        {
            Context.AttachCreature(GetComponent<Creature>());
        }

        rootNode?.Bind(Context);
        currentState = BHState.RUNNING;
        timeSinceLastTick = 0f;
        IsInitialized = rootNode != null;
    }

    public void ResetTree()
    {
        rootNode?.Reset();
        currentState = BHState.RUNNING;
        timeSinceLastTick = 0f;
    }

    private void TickTree(float deltaTime)
    {
        if (Context == null || rootNode == null)
        {
            currentState = BHState.FAILURE;
            return;
        }

        Context.DeltaTime = deltaTime;
        currentState = rootNode.Tick();
    }

    void OnDrawGizmos()
    {
        if(Context == null)
        {
            return;
        }

        //render path
        Context.TryGet(BehaviorTreeKeys.WanderPath, out List<Vector2> path);
        if (path != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
}