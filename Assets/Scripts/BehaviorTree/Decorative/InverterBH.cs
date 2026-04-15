public class InverterBH : DecorativeNodeBH
{
    public BHState stateReturnWhenChildIsNull = BHState.FAILURE;

    public override BHState OnUpdateState()
    {
        if (child == null)
        {
            return stateReturnWhenChildIsNull;
        }

        BHState childState = child.Tick();
        if (childState == BHState.SUCCESS)
        {
            return BHState.FAILURE;
        }
        else if (childState == BHState.FAILURE)
        {
            return BHState.SUCCESS;
        }

        return BHState.RUNNING;
    }
}