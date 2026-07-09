using UnityEngine;

[System.Serializable]
public class Armor_Comp : EntitiesComp
{
    public float Defense => (props as Armor_CompProperties)?.defense ?? 0f;
    public string ArmorSlot => (props as Armor_CompProperties)?.armorSlot ?? "chest";
    public float Durability => (props as Armor_CompProperties)?.durability ?? 100f;
    public float Weight => (props as Armor_CompProperties)?.weight ?? 1f;

    private float currentDurability;

    public override void Init()
    {
        currentDurability = Durability;
    }

    public float CalculateDamageReduction(float incomingDamage)
    {
        if (currentDurability <= 0) return 0f;
        
        float reduction = Defense / (Defense + 100f);
        float reducedDamage = incomingDamage * (1f - reduction);
        
        currentDurability -= 0.1f;
        return reducedDamage;
    }

    public bool IsBroken()
    {
        return currentDurability <= 0f;
    }

    public void Repair(float amount)
    {
        currentDurability = Mathf.Min(currentDurability + amount, Durability);
    }
}
