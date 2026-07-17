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
