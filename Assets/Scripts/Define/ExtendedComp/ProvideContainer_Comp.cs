using UnityEngine;

public class ProvideContainer_Comp : EntitiesComp
{
    private Container container;

    public Container OwnedContainer => container;

    public bool TryGetContainer(out Container targetContainer)
    {
        targetContainer = container;
        return targetContainer != null;
    }

    public override void Init()
    {
        Debug.Log("ProvideContainer_Comp Init for " + this.owner.gameObject.name);
        ContainerProperties containerProps = props as ContainerProperties;
        if(containerProps == null)
        {
            Debug.LogError("ContainerProperties is null for " + this.owner.gameObject.name);
            container = null;
            return;
        }
        else
        {
            container = new Container(containerProps.maxVolume, containerProps.maxWeight, containerProps.maxLength);
        }

        if (container != null)
        {
            Debug.Log(container.GetContainerInfo());
        }
    }
}
