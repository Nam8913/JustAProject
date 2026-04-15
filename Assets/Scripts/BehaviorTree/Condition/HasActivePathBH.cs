using System.Collections.Generic;
using UnityEngine;

public class HasActivePathBH : ConditionNodeBH
{
    public override BHState OnUpdateState()
    {
        if (Context == null)
        {
            return BHState.FAILURE;
        }

        if (!Context.TryGet(BehaviorTreeKeys.WanderPath, out List<Vector2> path) || path == null || path.Count == 0)
        {
            return BHState.FAILURE;
        }

        if (!Context.TryGet(BehaviorTreeKeys.WanderPathIndex, out int currentIndex))
        {
            currentIndex = 0;
        }

        return currentIndex < path.Count ? BHState.SUCCESS : BHState.FAILURE;
    }
}