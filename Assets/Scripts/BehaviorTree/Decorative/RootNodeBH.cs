public class RootNodeBH : DecorativeNodeBH
{
    public override BHState OnUpdateState()
    {
        if (child == null)
        {
            return BHState.FAILURE;
        }
        return child.Tick();
    }
}