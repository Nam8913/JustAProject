using UnityEngine;

public class Building : DefineThing
{
    private NeighborMask8 neighborMask = new NeighborMask8();

    public Direction8 NeighborMask
    {
        get => neighborMask.mask;
        set => neighborMask.mask = value;
    }

    public bool supportsNeighborConnection
    {
        get
        {
            return neighborMask.mask != Direction8.None;
        }
    }

    public override void Setup()
    {
        base.Setup();
        if (supportsNeighborConnection)
        {
            neighborMask = new NeighborMask8();
        }
    }

    /// <summary>
    /// Update neighbor mask dựa trên neighbors xung quanh.
    /// </summary>
    public void UpdateNeighborMask()
    {
        Vector2Int tilePos = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y));

        neighborMask.mask = BuildUtility.GetDirection8ForTile(tilePos);
    }

    /// <summary>
    /// Update sprite dựa trên neighbor mask hiện tại.
    /// </summary>
    public void UpdateSprite()
    {
        if (def == null) return;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        Sprite sprite = BuildUtility.GetSpriteForDefine(def, (Vector2)transform.position);
        if (sprite != null)
            renderer.sprite = sprite;
    }

    [System.Serializable]
    public struct NeighborMask8
    {
        public Direction8 mask;

        public NeighborMask8(Direction8 mask = Direction8.None) => this.mask = mask;

        public bool this[Direction8 dir]
        {
            get => (mask & dir) != 0;
            set => mask = value ? (mask | dir) : (mask & ~dir);
        }

        public byte ToByte() => (byte)mask;
        public override string ToString() => mask.ToString();
    }
}
