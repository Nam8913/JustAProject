using UnityEngine;

/// <summary>
/// CompProperties cho vật phẩm có thể đặt trong thế giới.
/// Xác định quy tắc đặt, snap grid, và cách tháo dỡ.
/// </summary>
[System.Serializable]
public class Placeable_CompProperties : CompProperties
{
    /// <summary>
    /// Có cần nền (foundation) để đặt không.
    /// true: chỉ đặt được trên nền đã xây.
    /// false: đặt được trên bất kỳ bề mặt nào.
    /// </summary>
    public bool requiresFoundation = false;

    /// <summary>
    /// Có snap vào grid khi đặt không.
    /// true: vật phẩm tự động căn chỉnh theo lưới.
    /// false: đặt tự do (như campfire).
    /// </summary>
    public bool snapToGrid = true;

    /// <summary>
    /// Khoảng cách tối đa có thể đặt (đơn vị world).
    /// VD: placementRange = 4 → player có thể đặt vật trong bán kính 4 đơn vị.
    /// </summary>
    public float placementRange = 3f;

    /// <summary>
    /// Có thể xoay vật phẩm khi đặt không.
    /// true: cho phép xoay 90° mỗi lần.
    /// false: giữ nguyên hướng mặc định.
    /// </summary>
    public bool canRotate = true;

    /// <summary>
    /// Khi tháo dỡ, vật phẩm có bị phá hủy không.
    /// true: tháo dỡ = phá hủy (mất vật phẩm).
    /// false: tháo dỡ = trả về inventory.
    /// </summary>
    public bool destroyOnRemove = true;
}
