using System.Collections.Generic;

public abstract class DecorativeNodeBH : NodeBehaviorTree
{
    public NodeBehaviorTree child;

    public void SetChild(NodeBehaviorTree newChild)
    {
        child = newChild;
    }

    protected override IEnumerable<NodeBehaviorTree> GetChildren()
    {
        if (child != null)
        {
            yield return child;
        }
    }
}
