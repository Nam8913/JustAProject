#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;
[System.Serializable]
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
        #if DEBUG_LOG_FLAG && false
        Debug.Log("ProvideContainer_Comp Init for " + this.owner.gameObject.name);
        #endif
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
            #if DEBUG_LOG_FLAG && false
            Debug.Log(container.GetContainerInfo());
            #endif
        }
    }
}
