public static class BehaviorTreeFactory
{
    public static RootNodeBH CreateDefaultCreatureTree(Creature creature)
    {
        SequenceBH followPathSequence = new SequenceBH();
        followPathSequence.AddChild(new HasActivePathBH());
        followPathSequence.AddChild(new FollowPathBH());

        SequenceBH requestPathSequence = new SequenceBH();
        requestPathSequence.AddChild(new AcquireRandomTargetBH());
        requestPathSequence.AddChild(new RequestPathBH());

        CooldownBH requestWithCooldown = new CooldownBH
        {
            cooldownSeconds = 0.5f,
            child = requestPathSequence
        };

        SelectorBH rootSelector = new SelectorBH();
        rootSelector.AddChild(followPathSequence);
        rootSelector.AddChild(requestWithCooldown);

        return new RootNodeBH
        {
            child = rootSelector
        };
    }
}