public class UntilBH : DecorativeNodeBH
{
    public BHState stateReturnWhenChildIsNull = BHState.FAILURE;
    public BHState RunUntilState = BHState.SUCCESS;

    public override BHState OnUpdateState()
    {
        if (child == null)
        {
            return stateReturnWhenChildIsNull;
        }

        BHState childState = child.Tick();
        if(childState == RunUntilState)
        {
            return BHState.SUCCESS;
        }
        if (childState == BHState.RUNNING)
        {
            return BHState.RUNNING;
        }

        return BHState.FAILURE;
    }
}