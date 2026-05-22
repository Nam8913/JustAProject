using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Container
{
    public readonly float maxVolume;
    public readonly float maxWeight;
    public readonly float maxLength;

    public float currentVolume = 0f;
    public float currentWeight = 0f;

    public Container parent;
    [System.NonSerialized]
    public List<Container> children;

    public List<ItemContainer> items;

    public event Action Changed;

    public Container(float maxVolume, float maxWeight, float maxLength)
    {
        this.maxVolume = maxVolume;
        this.maxWeight = maxWeight;
        this.maxLength = maxLength;
        this.children = new List<Container>();
        this.items = new List<ItemContainer>();
    }

    public Container(float maxVolume, float maxWeight, float maxLength, Container parent) : this(maxVolume, maxWeight, maxLength)
    {
        this.parent = parent;
    }

    public virtual bool IsVirtual => false;
    public bool hasAnyItems => items != null && items.Count > 0;
    public bool hasChildContainers => children != null && children.Count > 0;

    public void AddChildContainer(Container child)
    {
        if (child == null)
        {
            return;
        }

        if (children == null)
        {
            children = new List<Container>();
        }

        if (!children.Contains(child))
        {
            children.Add(child);
            child.parent = this;
            NotifyChanged();
        }
    }

    public bool RemoveChildContainer(Container child)
    {
        if (child == null || children == null)
        {
            return false;
        }

        bool removed = children.Remove(child);
        if (removed)
        {
            if (child.parent == this)
            {
                child.parent = null;
            }

            NotifyChanged();
        }

        return removed;
    }

    public bool CanAddItem(string itemId, int quantity = 1)
    {
        if (quantity <= 0)
        {
            return false;
        }

        return GetMaxAddableQuantity(itemId) >= quantity;
    }

    public int GetMaxAddableQuantity(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return 0;
        }

        Define itemDef = DatabaseThing.GetData<Define>(itemId);
        if (itemDef == null)
        {
            return 0;
        }

        if (itemDef.length > maxLength)
        {
            return 0;
        }

        if (float.IsPositiveInfinity(maxVolume) && float.IsPositiveInfinity(maxWeight))
        {
            return int.MaxValue;
        }

        float remainingVolume = maxVolume - currentVolume;
        float remainingWeight = maxWeight - currentWeight;

        int volumeLimit;
        if (float.IsPositiveInfinity(remainingVolume))
        {
            volumeLimit = int.MaxValue;
        }
        else if (itemDef.volume > 0f)
        {
            volumeLimit = Mathf.FloorToInt(Mathf.Max(0f, remainingVolume) / itemDef.volume);
        }
        else
        {
            volumeLimit = int.MaxValue;
        }

        int weightLimit;
        if (float.IsPositiveInfinity(remainingWeight))
        {
            weightLimit = int.MaxValue;
        }
        else if (itemDef.mass > 0f)
        {
            weightLimit = Mathf.FloorToInt(Mathf.Max(0f, remainingWeight) / itemDef.mass);
        }
        else
        {
            weightLimit = int.MaxValue;
        }

        return Mathf.Max(0, Mathf.Min(volumeLimit, weightLimit));
    }

    public int GetItemQuantity(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || items == null)
        {
            return 0;
        }

        int totalQuantity = 0;
        foreach (ItemContainer item in items)
        {
            if (item == null || item.itemId != itemId)
            {
                continue;
            }

            totalQuantity += Mathf.Max(0, item.quantity);
        }

        return totalQuantity;
    }

    public bool TryAddItem(string itemId , int quantity = 1)
    {
        if (!CanAddItem(itemId, quantity))
        {
            if (quantity <= 0)
            {
                Debug.LogError("Quantity must be greater than zero.");
            }
            else
            {
                Debug.LogWarning($"Cannot add item {itemId} to container. Not enough capacity.");
            }

            return false;
        }

        Define itemDef = DatabaseThing.GetData<Define>(itemId);
        if (itemDef == null)
        {
            Debug.LogError($"Item with ID {itemId} not found in database.");
            return false;
        }

        if (items == null)
        {
            items = new List<ItemContainer>();
        }

        int remainingQuantity = quantity;
        int maxStackCount = Mathf.Max(1, itemDef.maxStackCount);

        if (maxStackCount > 1)
        {
            List<ItemContainer> existingStacks = items.FindAll(item => item != null && item.itemId == itemId && item.quantity < maxStackCount);
            foreach (ItemContainer stack in existingStacks)
            {
                if (remainingQuantity <= 0)
                {
                    break;
                }

                int availableSpace = maxStackCount - stack.quantity;
                int addQuantity = Mathf.Min(availableSpace, remainingQuantity);
                stack.quantity += addQuantity;
                remainingQuantity -= addQuantity;
            }

            while (remainingQuantity > 0)
            {
                int newStackQuantity = Mathf.Min(maxStackCount, remainingQuantity);
                ItemContainer newItem = new ItemContainer(this, itemId, newStackQuantity);
                items.Add(newItem);
                remainingQuantity -= newStackQuantity;
            }
        }
        else
        {
            while (remainingQuantity > 0)
            {
                ItemContainer newItem = new ItemContainer(this, itemId, 1);
                items.Add(newItem);
                remainingQuantity--;
            }
        }

        RefreshTotals();
        return true;
    }

    public bool TryRemoveItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId) || quantity <= 0 || items == null)
        {
            return false;
        }

        if (GetItemQuantity(itemId) < quantity)
        {
            return false;
        }

        int remainingQuantity = quantity;
        for (int index = items.Count - 1; index >= 0 && remainingQuantity > 0; index--)
        {
            ItemContainer item = items[index];
            if (item == null || item.itemId != itemId)
            {
                continue;
            }

            int removableQuantity = Mathf.Min(item.quantity, remainingQuantity);
            item.quantity -= removableQuantity;
            remainingQuantity -= removableQuantity;

            if (item.quantity <= 0)
            {
                items.RemoveAt(index);
            }
        }

        RefreshTotals();
        return remainingQuantity <= 0;
    }

    // public int MoveItemTo(Container targetContainer, string itemId, int quantity = 1)
    // {
    //     if (targetContainer == null || quantity <= 0)
    //     {
    //         return 0;
    //     }

    //     if (ReferenceEquals(this, targetContainer))
    //     {
    //         return 0;
    //     }

    //     if (this is NearbyTilesContainer nearbySource)
    //     {
    //         return nearbySource.MoveItemTo(targetContainer, itemId, quantity);
    //     }

    //     if (targetContainer is NearbyTilesContainer nearbyTarget)
    //     {
    //         return nearbyTarget.ReceiveItemFrom(this, itemId, quantity);
    //     }

    //     int transferableQuantity = Mathf.Min(
    //         quantity,
    //         Mathf.Min(GetItemQuantity(itemId), targetContainer.GetMaxAddableQuantity(itemId)));
    //     if (transferableQuantity <= 0)
    //     {
    //         return 0;
    //     }

    //     if (!TryRemoveItem(itemId, transferableQuantity))
    //     {
    //         return 0;
    //     }

    //     if (targetContainer.TryAddItem(itemId, transferableQuantity))
    //     {
    //         return transferableQuantity;
    //     }

    //     TryAddItem(itemId, transferableQuantity);
    //     return 0;
    // }

    // public bool TryTransferItemTo(Container targetContainer, string itemId, int quantity = 1)
    // {
    //     return MoveItemTo(targetContainer, itemId, quantity) == quantity;
    // }

    // public bool TryTransferItemTo(IContainerOwner targetOwner, string itemId, int quantity = 1)
    // {
    //     if (targetOwner == null || !targetOwner.TryGetContainer(out Container targetContainer))
    //     {
    //         return false;
    //     }

    //     return TryTransferItemTo(targetContainer, itemId, quantity);
    // }

    public void RefreshTotals()
    {
        currentVolume = 0f;
        currentWeight = 0f;

        if (items == null)
        {
            return;
        }

        foreach (ItemContainer item in items)
        {
            if (item == null)
            {
                continue;
            }

            Define itemDef = item.itemDef ?? DatabaseThing.GetData<Define>(item.itemId);
            if (itemDef == null)
            {
                continue;
            }

            int itemQuantity = Mathf.Max(0, item.quantity);
            currentVolume += itemDef.volume * itemQuantity;
            currentWeight += itemDef.mass * itemQuantity;
        }

        NotifyChanged();
    }

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }

    public string GetContainerInfo()
    {
        string str = string.Empty;
        str += $"Container Info: Current Volume: {currentVolume}/{maxVolume}, Current Weight: {currentWeight}/{maxWeight}, Max Length: {maxLength}\n";
        str += $"Items in Container:\n";
        if (items != null)
        {
            foreach(var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                str += $"- {item.GetInfo()}\n";
            }
        }
        if(children != null && children.Count > 0)
        {
            str += $"Child Containers:\n";
            foreach(var child in children)
            {
                str += $"- {child.GetContainerInfo()}\n";
            }
        }
        return str;
    }
}
