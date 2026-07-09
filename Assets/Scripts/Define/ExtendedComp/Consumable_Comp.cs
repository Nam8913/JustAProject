using UnityEngine;

[System.Serializable]
public class Consumable_Comp : EntitiesComp
{
    public float HealAmount => (props as Consumable_CompProperties)?.healAmount ?? 0f;
    public float HungerAmount => (props as Consumable_CompProperties)?.hungerAmount ?? 0f;
    public float ThirstAmount => (props as Consumable_CompProperties)?.thirstAmount ?? 0f;
    public float Duration => (props as Consumable_CompProperties)?.duration ?? 0f;
    public string EffectId => (props as Consumable_CompProperties)?.effectId ?? "";

    public bool CanConsume()
    {
        return true;
    }

    public void Consume()
    {
        if (owner == null) return;
        
        // Apply effects to the consumer
        // This will be connected to the creature's stats system
        Debug.Log($"Consumed {Def?.name}: Heal={HealAmount}, Hunger={HungerAmount}, Thirst={ThirstAmount}");
    }
}
