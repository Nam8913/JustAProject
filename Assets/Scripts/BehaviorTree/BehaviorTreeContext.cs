using System.Collections.Generic;
using UnityEngine;

public sealed class BehaviorTreeContext
{
    private readonly Dictionary<string, object> blackboard = new Dictionary<string, object>();

    public BehaviorTreeContext(BehaviorTree tree, Creature creature = null)
    {
        Tree = tree;

        if (creature != null)
        {
            Creature = creature;
        }
        else if (tree != null)
        {
            Creature = tree.GetComponent<Creature>();
        }
    }

    public BehaviorTree Tree { get; private set; }
    public Creature Creature { get; private set; }
    public GameObject GameObject => Tree != null ? Tree.gameObject : (Creature != null ? Creature.gameObject : null);
    public Transform Transform => Tree != null ? Tree.transform : (Creature != null ? Creature.transform : null);
    public GameService Game => GameService.Ins;
    public NavService Navigation => GameService.Ins.Navigation;
    public float DeltaTime { get; internal set; }

    public void AttachCreature(Creature creature)
    {
        Creature = creature;
    }

    public void Set<T>(string key, T value)
    {
        blackboard[key] = value;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (blackboard.TryGetValue(key, out object storedValue) && storedValue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default(T);
        return false;
    }

    public T GetOrDefault<T>(string key, T defaultValue = default(T))
    {
        return TryGet(key, out T value) ? value : defaultValue;
    }

    public bool Contains(string key)
    {
        return blackboard.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return blackboard.Remove(key);
    }

    public void Clear()
    {
        blackboard.Clear();
    }
}