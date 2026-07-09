using UnityEngine;

[System.Serializable]
public class Placeable_Comp : EntitiesComp
{
    public bool RequiresFoundation => (props as Placeable_CompProperties)?.requiresFoundation ?? false;
    public bool SnapToGrid => (props as Placeable_CompProperties)?.snapToGrid ?? true;
    public float PlacementRange => (props as Placeable_CompProperties)?.placementRange ?? 3f;
    public bool CanRotate => (props as Placeable_CompProperties)?.canRotate ?? true;
    public bool DestroyOnRemove => (props as Placeable_CompProperties)?.destroyOnRemove ?? true;

    public bool CanPlace(Vector3 position)
    {
        // Check if position is valid for placement
        // This would check for obstacles, foundation requirements, etc.
        return true;
    }

    public void Place(Vector3 position, Quaternion rotation)
    {
        if (owner == null) return;
        
        owner.transform.position = position;
        owner.transform.rotation = rotation;
        
        Debug.Log($"Placed {Def?.name} at {position}");
    }

    public void Remove()
    {
        if (owner == null) return;
        
        if (DestroyOnRemove)
        {
            Object.Destroy(owner.gameObject);
        }
        else
        {
            // Return to inventory or disable
            owner.gameObject.SetActive(false);
        }
    }
}
