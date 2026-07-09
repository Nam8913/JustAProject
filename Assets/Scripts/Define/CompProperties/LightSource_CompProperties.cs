using UnityEngine;

/// <summary>
/// CompProperties cho vật phát sáng (đèn, đuốc, lò sưởi).
/// Tạo Point Light tự động khi Init() và quản lý ánh sáng.
/// </summary>
[System.Serializable]
public class LightSource_CompProperties : CompProperties
{
    /// <summary>
    /// Cường độ ánh sáng (lumens).
    /// VD: intensity = 2.0 = ánh sáng gấp 2 lần mặc định.
    /// </summary>
    public float intensity = 1f;

    /// <summary>
    /// Bán kính chiếu sáng (đơn vị world).
    /// VD: range = 5 = ánh sáng tỏa ra 5 đơn vị quanh nguồn sáng.
    /// </summary>
    public float range = 5f;

    /// <summary>
    /// Màu ánh sáng (RGB).
    /// VD: white = ánh sáng trắng, orange = ánh sáng ấm.
    /// </summary>
    public UnityEngine.Vector3 lightColor = UnityEngine.Vector3.one;

    /// <summary>
    /// Có cần nhiên liệu để duy trì ánh sáng không.
    /// true: sẽ tiêu hao nhiên liệu theo fuelConsumptionRate.
    /// false: sáng vĩnh viễn.
    /// </summary>
    public bool requiresFuel = false;

    /// <summary>
    /// Tốc độ tiêu hao nhiên liệu (đơn vị/giây).
    /// VD: fuelConsumptionRate = 0.1 → tiêu 0.1 nhiên liệu mỗi giây.
    /// Chỉ hoạt động khi requiresFuel = true.
    /// </summary>
    public float fuelConsumptionRate = 0.1f;
}
