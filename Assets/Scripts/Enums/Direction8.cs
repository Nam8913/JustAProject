[System.Flags]
public enum Direction8 : byte
{
    None = 0,      // 00000000
    TopLeft = 1,      // 00000001
    Top = 2,      // 00000010
    TopRight = 4,      // 00000100
    Left = 8,      // 00001000
    Right = 16,     // 00010000
    BottomLeft = 32,     // 00100000
    Bottom = 64,     // 01000000
    BottomRight = 128,     // 10000000
    All = 255     // 11111111
}