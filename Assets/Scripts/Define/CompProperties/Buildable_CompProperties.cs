using UnityEngine;

/// <summary>
/// CompProperties cho vật phẩm có thể xây dựng/nội thất.
/// Xác định kích thước, hướng xoay, khả năng tháo dỡ/sửa chữa.
/// </summary>
[System.Serializable]
public class Buildable_CompProperties : CompProperties
{
    /// <summary>
    /// Có thể xoay vật phẩm khi đặt không.
    /// true: cho phép xoay 90° mỗi lần.
    /// false: giữ nguyên hướng mặc định.
    /// </summary>
    public bool rotatable = false;

    /// <summary>
    /// Có thể tháo dỡ (uninstall) vật phẩm đã đặt không.
    /// true: có thể tháo → trả về inventory.
    /// false: không thể tháo (phải phá hủy).
    /// </summary>
    public bool uninstallable = false;

    /// <summary>
    /// Có thể sửa chữa vật phẩm bị hư không.
    /// true: sử dụng vật liệu repair → khôi phục HP.
    /// false: không thể sửa.
    /// </summary>
    public bool repairable = false;

    /// <summary>
    /// Kích thước chiếm chỗ trên lưới (x = chiều rộng, y = chiều cao).
    /// VD: (1,1) = chiếm 1 ô, (2,1) = chiếm 2 ô ngang.
    /// Ảnh hưởng đến việc đặt và va chạm.
    /// </summary>
    public Vector2Int size = Vector2Int.one;
}
