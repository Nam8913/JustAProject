public class RepeaterBH : DecorativeNodeBH
{
    public int repeatTimes = 1;
    public bool countIncludeRunning = false;
    public bool repeatForever = false;

    private int currentRepeatTimes = 0;

    public override void OnEnter()
    {
        currentRepeatTimes = 0;
    }

    public override BHState OnUpdateState()
    {
        if (child == null)
        {
            return BHState.FAILURE;
        }

        if (!repeatForever && repeatTimes == 0)
        {
            return BHState.SUCCESS;
        }

        if (!repeatForever && currentRepeatTimes >= repeatTimes)
        {
            return BHState.SUCCESS;
        }

        BHState childState = child.Tick();
        if (childState == BHState.RUNNING && !countIncludeRunning)
        {
            return BHState.RUNNING;
        }

        if (childState == BHState.SUCCESS || childState == BHState.FAILURE || countIncludeRunning)
        {
            currentRepeatTimes++;
        }

        if (!repeatForever && currentRepeatTimes >= repeatTimes)
        {
            return BHState.SUCCESS;
        }

        return BHState.RUNNING;
    }
}