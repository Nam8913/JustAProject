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

        Vector2 origin = Context.Creature.WorldPosition;
        Vector2 targetPosition = origin + Random.insideUnitCircle * wanderRadius;

        Context.Set(BehaviorTreeKeys.WanderTarget, targetPosition);
        return BHState.SUCCESS;
    }
}