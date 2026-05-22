public sealed class ItemContainer
{
    private Container holder;
    public string itemId;
    public Define itemDef;
    public int quantity = 1;

    public ItemContainer(Container holder, string itemId, int quantity = 1)
    {
        this.holder = holder;
        this.itemId = itemId;
        this.itemDef = DatabaseThing.GetData<Define>(itemId);
        this.quantity = quantity;
    }

    public void UpdateContainer()
    {
        holder?.RefreshTotals();
    }

    public string GetInfo()
    {
        return $"ID:{itemId} {itemDef.name}: {quantity}";
    }
}
