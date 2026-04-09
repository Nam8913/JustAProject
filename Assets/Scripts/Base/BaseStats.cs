using UnityEngine;

public static class BaseStats
{
    //Defensive Stats
    public static Status HP => new Status("HP", "Health Points", 0f);
    public static Status MP => new Status("MP", "Mana Points", 0f);
    public static Status AR => new Status("AR", "Armor", 0f);
    public static Status MR => new Status("MR", "Magic Resistance", 0f);
    public static Status HPRegen => new Status("HPRegen", "Health Points Regeneration", 0f);
    public static Status MPRegen => new Status("MPRegen", "Mana Points Regeneration", 0f);
    //Offensive Stats
    public static Status MS => new Status("MS", "Movement Speed", 0f);
    public static Status AD => new Status("AD", "Attack Damage", 0f);
    public static Status AS => new Status("AS", "Attack Speed", 0f);
    public static Status AP => new Status("AP", "Ability Power", 0f);
    public static Status CriticalChance => new Status("CriticalChance", "Chance to deal critical damage", 0f);
    public static Status CriticalDamageBonus => new Status("CriticalDamageBonus", "Bonus damage dealt on critical hits", 0f);
    public static Status CR => new Status("CR", "Cooldown Reduction", 0f);
    public static Status LifeSteal => new Status("LifeSteal", "Percentage of damage dealt that is returned as health", 0f);
    public static Status SpellVamp => new Status("SpellVamp", "Percentage of damage dealt by abilities that is returned as health", 0f);
    public static Status ArmorPenetration => new Status("ArmorPenetration", "Reduces the target's armor", 0f);
    public static Status MagicPenetration => new Status("MagicPenetration", "Reduces the target's magic resistance", 0f);
    public static Status AttackRange => new Status("AttackRange", "Range of the character's attacks", 0f);
    public static void AddAllBaseStatus(StatsHolder stats)
    {
        //Defensive Stats
        stats.AddStatus(HP);
        stats.AddStatus(MP);
        stats.AddStatus(AR);
        stats.AddStatus(MR);
        stats.AddStatus(HPRegen);
        stats.AddStatus(MPRegen);
        //Offensive Stats
        stats.AddStatus(MS);
        stats.AddStatus(AD);
        stats.AddStatus(AS);
        stats.AddStatus(AP);
        stats.AddStatus(CriticalChance);
        stats.AddStatus(CriticalDamageBonus);
        stats.AddStatus(CR);
        stats.AddStatus(LifeSteal);
        stats.AddStatus(SpellVamp);
        stats.AddStatus(ArmorPenetration);
        stats.AddStatus(MagicPenetration);
        stats.AddStatus(AttackRange);
    }
}