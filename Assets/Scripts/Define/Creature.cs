using UnityEngine;

public class Creature : DefineThing
{
    StatsHolder statsHolder;
    TraitsHolder traitsHolder;
    
    public Creature()
    {
        labelName = "DefaultCreature";
        labelDescription = "A creature with no special traits and no special abilities just for test.";
        statsHolder = new StatsHolder();
        BaseStats.AddAllBaseStatus(statsHolder);
        traitsHolder = new TraitsHolder();
    }

    public override void ConfigError()
    {
        base.ConfigError();
        Debug.Log(statsHolder.DebugString());
        Debug.Log(traitsHolder.DebugString());
        // Additional error checks for Creature can be added here
    }
}
