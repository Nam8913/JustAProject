using System.Collections.Generic;
using UnityEngine;

public class FollowPathBH : ActionNodeBH
{
    public float moveSpeed = 0f;
    public float waypointReachDistance = 0.15f;

    public override void OnEnter()
    {
       moveSpeed = Context.Creature.Stats.MS;
    }

    public override BHState OnUpdateState()
    {
        if (Context == null || Context.Creature == null)
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

        if (currentIndex >= path.Count)
        {
            Context.Remove(BehaviorTreeKeys.WanderPath);
            Context.Remove(BehaviorTreeKeys.WanderPathIndex);
            Context.Remove(BehaviorTreeKeys.WanderPathTarget);
            return BHState.SUCCESS;
        }

        Vector2 currentTarget = path[currentIndex];
        Transform creatureTransform = Context.Creature.transform;
        creatureTransform.position = Vector2.MoveTowards(creatureTransform.position, currentTarget, moveSpeed * Context.DeltaTime);

        if (Vector2.Distance(creatureTransform.position, currentTarget) <= waypointReachDistance)
        {
            currentIndex++;
            if (currentIndex >= path.Count)
            {
                Context.Remove(BehaviorTreeKeys.WanderPath);
                Context.Remove(BehaviorTreeKeys.WanderPathIndex);
                Context.Remove(BehaviorTreeKeys.WanderPathTarget);
                return BHState.SUCCESS;
            }
        }

        Context.Set(BehaviorTreeKeys.WanderPathIndex, currentIndex);
        return BHState.RUNNING;
    }
}