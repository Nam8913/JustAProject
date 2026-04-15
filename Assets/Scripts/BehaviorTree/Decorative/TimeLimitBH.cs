using UnityEngine;

public class TimeLimitBH : DecorativeNodeBH
{
    public float timeLimit = 1f;
    private float timeElapsed = 0f;

    public override void OnEnter()
    {
        timeElapsed = 0f;
    }

    public override BHState OnUpdateState()
    {
        if (child == null)
        {
            return BHState.FAILURE;
        }

        timeElapsed += Context != null ? Context.DeltaTime : Time.deltaTime;
        if (timeElapsed >= timeLimit)
        {
            return BHState.FAILURE;
        }

        return child.Tick();
    }
}