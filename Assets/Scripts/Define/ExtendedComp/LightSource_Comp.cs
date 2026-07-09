using UnityEngine;

[System.Serializable]
public class LightSource_Comp : EntitiesComp
{
    private Light pointLight;
    private float currentFuel;
    private bool isOn = true;

    public float Intensity => (props as LightSource_CompProperties)?.intensity ?? 1f;
    public float Range => (props as LightSource_CompProperties)?.range ?? 5f;
    public Vector3 LightColor => (props as LightSource_CompProperties)?.lightColor ?? Vector3.one;
    public bool RequiresFuel => (props as LightSource_CompProperties)?.requiresFuel ?? false;
    public float FuelConsumptionRate => (props as LightSource_CompProperties)?.fuelConsumptionRate ?? 0.1f;

    public override void Init()
    {
        if (owner == null) return;
        
        // Create light component
        GameObject lightObj = new GameObject("PointLight");
        lightObj.transform.SetParent(owner.transform);
        lightObj.transform.localPosition = Vector3.zero;
        
        pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.intensity = Intensity;
        pointLight.range = Range;
        pointLight.color = new Color(LightColor.x, LightColor.y, LightColor.z);
        
        currentFuel = 100f;
    }

    public override void Update()
    {
        if (!isOn || pointLight == null) return;
        
        if (RequiresFuel)
        {
            currentFuel -= FuelConsumptionRate * Time.deltaTime;
            if (currentFuel <= 0f)
            {
                TurnOff();
            }
        }
    }

    public void TurnOn()
    {
        isOn = true;
        if (pointLight != null)
        {
            pointLight.enabled = true;
        }
    }

    public void TurnOff()
    {
        isOn = false;
        if (pointLight != null)
        {
            pointLight.enabled = false;
        }
    }

    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Max(0f, currentFuel + amount);
        if (currentFuel > 0f && !isOn)
        {
            TurnOn();
        }
    }

    public float GetFuelLevel()
    {
        return currentFuel;
    }
}
