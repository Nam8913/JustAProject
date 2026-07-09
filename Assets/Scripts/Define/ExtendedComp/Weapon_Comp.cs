using UnityEngine;

[System.Serializable]
public class Weapon_Comp : EntitiesComp
{
    public float Damage => (props as Weapon_CompProperties)?.damage ?? 10f;
    public float AttackSpeed => (props as Weapon_CompProperties)?.attackSpeed ?? 1f;
    public float Range => (props as Weapon_CompProperties)?.range ?? 1.5f;
    public string WeaponType => (props as Weapon_CompProperties)?.weaponType ?? "melee";
    public float CriticalChance => (props as Weapon_CompProperties)?.criticalChance ?? 0.05f;
    public float CriticalMultiplier => (props as Weapon_CompProperties)?.criticalMultiplier ?? 2f;

    public float CalculateDamage()
    {
        float baseDamage = Damage;
        if (Random.value < CriticalChance)
        {
            baseDamage *= CriticalMultiplier;
        }
        return baseDamage;
    }

    public bool CanAttack()
    {
        return true;
    }
}
