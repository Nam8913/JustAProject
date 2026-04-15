using System.Collections.Generic;
using UnityEngine;

public abstract class NodeBehaviorTree
{
    public NodeBehaviorTree Parent { get; private set; }
    public BehaviorTreeContext Context { get; private set; }
    public BHState CurrentState { get; private set; } = BHState.FAILURE;
    public bool IsStarted { get; private set; }

    public virtual void OnEnter()
    {
    }

    public abstract BHState OnUpdateState();

    public virtual void OnExit()
    {
    }

    public void Bind(BehaviorTreeContext context)
    {
        Context = context;
        OnBind();

        foreach (var child in GetChildren())
        {
            if (child == null)
            {
                continue;
            }

            child.Parent = this;
            child.Bind(context);
        }
    }

    public virtual void Reset()
    {
        foreach (var child in GetChildren())
        {
            child?.Reset();
        }

        if (IsStarted)
        {
            OnExit();
        }

        IsStarted = false;
        CurrentState = BHState.FAILURE;
    }

    public BHState Tick()
    {
        if (Context == null)
        {
            Debug.LogError($"{GetType().Name} was ticked without a BehaviorTreeContext.");
            return BHState.FAILURE;
        }

        if (!IsStarted)
        {
            OnEnter();
            IsStarted = true;
        }

        CurrentState = OnUpdateState();

        if (CurrentState == BHState.SUCCESS || CurrentState == BHState.FAILURE)
        {
            OnExit();
            IsStarted = false;
        }

        return CurrentState;
    }

    protected virtual void OnBind()
    {
    }

    protected virtual IEnumerable<NodeBehaviorTree> GetChildren()
    {
        yield break;
    }
}
