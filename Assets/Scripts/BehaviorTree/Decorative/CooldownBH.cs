using UnityEngine;

public class CooldownBH : DecorativeNodeBH
{
    public float cooldownSeconds = 1f;
    public BHState stateWhileCooling = BHState.RUNNING;

    private float cooldownRemaining = 0f;

    public override BHState OnUpdateState()
    {
        if (child == null)
        {
            return BHState.FAILURE;
        }

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Context != null ? Context.DeltaTime : Time.deltaTime;
            if (cooldownRemaining > 0f)
            {
                return stateWhileCooling;
            }

            cooldownRemaining = 0f;
        }

        BHState childState = child.Tick();
        if (childState == BHState.RUNNING)
        {
            return BHState.RUNNING;
        }

        cooldownRemaining = cooldownSeconds;
        return childState;
    }
}