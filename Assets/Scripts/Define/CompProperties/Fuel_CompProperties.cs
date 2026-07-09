using UnityEngine;

/// <summary>
/// CompProperties cho vật phẩm có thể làm nhiên liệu (gỗ, than, dầu...).
/// Được sử dụng bởi LightSource_Comp và các hệ thống cần đốt cháy.
/// </summary>
[System.Serializable]
public class Fuel_CompProperties : CompProperties
{
    /// <summary>
    /// Tổng lượng nhiên liệu cung cấp khi đốt hết.
    /// VD: fuelAmount = 50 → đủ để đốt trong 50 đơn vị thời gian.
    /// </summary>
    public float fuelAmount = 10f;

    /// <summary>
    /// Thời gian đốt tối đa (giây).
    /// VD: burnTime = 60 → vật phẩm cháy trong 60 giây.
    /// heatOutput / burnTime = nhiệt lượng mỗi giây.
    /// </summary>
    public float burnTime = 10f;

    /// <summary>
    /// Tổng nhiệt lượng tỏa ra khi đốt hết.
    /// heatOutput / burnTime = nhiệt mỗi giây.
    /// Ảnh hưởng đến tốc độ nấu nướng/sưởi ấm.
    /// </summary>
    public float heatOutput = 1f;
}
