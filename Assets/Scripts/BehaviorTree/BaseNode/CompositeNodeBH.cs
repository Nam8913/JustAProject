using System.Collections.Generic;

public abstract class CompositeNodeBH : NodeBehaviorTree
{
    public List<NodeBehaviorTree> childs = new List<NodeBehaviorTree>();
    protected int currentChildIndex = 0;

    public void AddChild(NodeBehaviorTree child)
    {
        if (child != null)
        {
            childs.Add(child);
        }
    }

    protected override IEnumerable<NodeBehaviorTree> GetChildren()
    {
        return childs;
    }

    public override void OnEnter()
    {
        currentChildIndex = 0;
    }

    public override void OnExit()
    {
        currentChildIndex = 0;
    }

    public override void Reset()
    {
        currentChildIndex = 0;
        base.Reset();
    }
}
