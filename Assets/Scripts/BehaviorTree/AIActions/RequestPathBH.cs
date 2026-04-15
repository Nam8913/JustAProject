using System.Collections.Generic;
using UnityEngine;

public class RequestPathBH : ActionNodeBH
{
    public override BHState OnUpdateState()
    {
        if (Context == null || Context.Creature == null)
        {
            return BHState.FAILURE;
        }

        if (!Context.TryGet(BehaviorTreeKeys.WanderTarget, out Vector2 targetPosition))
        {
            return BHState.FAILURE;
        }

        List<Vector2> path = Context.Navigation.GetPathFromTo(Context.Creature, targetPosition);
        if (path == null || path.Count == 0)
        {
            Context.Remove(BehaviorTreeKeys.WanderPath);
            Context.Remove(BehaviorTreeKeys.WanderPathIndex);
            return BHState.FAILURE;
        }

        Context.Set(BehaviorTreeKeys.WanderPath, path);
        Context.Set(BehaviorTreeKeys.WanderPathIndex, 0);
        return BHState.SUCCESS;
    }
}