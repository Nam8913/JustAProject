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

        // If there is already an active path for the same target, do not rebuild it.
        if (Context.TryGet(BehaviorTreeKeys.WanderPath, out List<Vector2> activePath) &&
            activePath != null &&
            activePath.Count > 0 &&
            Context.TryGet(BehaviorTreeKeys.WanderPathTarget, out Vector2 lockedTarget) &&
            lockedTarget == targetPosition)
        {
            return BHState.SUCCESS;
        }

        List<Vector2> path = Context.Navigation.GetPathFromTo(Context.Creature, targetPosition);
        if (path == null || path.Count == 0)
        {
            return BHState.FAILURE;
        }

        Context.Set(BehaviorTreeKeys.WanderPath, path);
        Context.Set(BehaviorTreeKeys.WanderPathIndex, 0);
        Context.Set(BehaviorTreeKeys.WanderPathTarget, targetPosition);
        return BHState.SUCCESS;
    }
}