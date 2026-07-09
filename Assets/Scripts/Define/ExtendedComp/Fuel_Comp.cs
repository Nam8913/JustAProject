using UnityEngine;

[System.Serializable]
public class Fuel_Comp : EntitiesComp
{
    public float FuelAmount => (props as Fuel_CompProperties)?.fuelAmount ?? 10f;
    public float BurnTime => (props as Fuel_CompProperties)?.burnTime ?? 10f;
    public float HeatOutput => (props as Fuel_CompProperties)?.heatOutput ?? 1f;

    public float GetBurnDuration()
    {
        return BurnTime;
    }

    public float GetHeatPerSecond()
    {
        return HeatOutput / BurnTime;
    }

    public bool IsCombustible()
    {
        return FuelAmount > 0f;
    }
}
