public class SequenceBH : CompositeNodeBH
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

            if (childState == BHState.FAILURE)
            {
                return BHState.FAILURE;
            }

            currentChildIndex++;
        }

        return BHState.SUCCESS;
    }
}
