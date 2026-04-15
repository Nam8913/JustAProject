public class SelectorBH : CompositeNodeBH
{
    public override BHState OnUpdateState()
    {
        while (currentChildIndex < childs.Count)
        {
            NodeBehaviorTree child = childs[currentChildIndex];
            if (child == null)
            {
                return BHState.FAILURE;
            }

            BHState childState = child.Tick();
            if (childState == BHState.RUNNING)
            {
                return BHState.RUNNING;
            }

            if (childState == BHState.SUCCESS)
            {
                return BHState.SUCCESS;
            }

            currentChildIndex++;
        }

        return BHState.FAILURE;
    }
}
