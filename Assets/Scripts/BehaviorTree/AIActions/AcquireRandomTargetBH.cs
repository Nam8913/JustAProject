using System.Collections.Generic;
using UnityEngine;

public class AcquireRandomTargetBH : ActionNodeBH
{
    public float wanderRadius = 10f;

    public override BHState OnUpdateState()
    {
        if (Context == null || Context.Creature == null)
        {
            return BHState.FAILURE;
        }

        // If the creature is already following a path, keep the current target unchanged.
        // This prevents the request branch from accidentally replacing an in-progress route.
        if (Context.TryGet(BehaviorTreeKeys.WanderPath, out List<Vector2> activePath) && activePath != null && activePath.Count > 0)
        {
            return BHState.SUCCESS;
        }

        Vector2 origin = Context.Creature.WorldPosition;
        Vector2 targetPosition = origin + Random.insideUnitCircle * wanderRadius;

        Context.Set(BehaviorTreeKeys.WanderTarget, targetPosition);
        return BHState.SUCCESS;
    }
}