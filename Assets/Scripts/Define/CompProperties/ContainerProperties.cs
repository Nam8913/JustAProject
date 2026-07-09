using UnityEngine;

/// <summary>
/// CompProperties cho vật phẩm có thể chứa đồ (túi, rương, thùng...).
/// Xác định giới hạn trọng lượng, thể tích, độ dài tối đa.
/// </summary>
[System.Serializable]
public class ContainerProperties : CompProperties
{
    /// <summary>
    /// Trọng lượng tối đa có thể chứa (đơn vị: kg hoặc gram tùy game).
    /// VD: maxWeight = 100 → chứa được tổng cộng 100 đơn vị trọng lượng.
    /// = 0: không giới hạn trọng lượng.
    /// </summary>
    public float maxWeight = 0;

    /// <summary>
    /// Thể tích tối đa có thể chứa (đơn vị: L hoặc cm³ tùy game).
    /// VD: maxVolume = 50 → chứa được tổng cộng 50 đơn vị thể tích.
    /// = 0: không giới hạn thể tích.
    /// </summary>
    public float maxVolume = 0;

    /// <summary>
    /// Chiều dài tối đa của 1 item có thể chứa (đơn vị: m hoặc cm).
    /// VD: maxLength = 2 → chỉ chứa được items có length ≤ 2.
    /// = 0: không giới hạn độ dài.
    /// </summary>
    public float maxLength = 0;
}
