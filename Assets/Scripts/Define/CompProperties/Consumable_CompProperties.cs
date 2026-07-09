using UnityEngine;

/// <summary>
/// CompProperties cho vật phẩm có thể tiêu thụ (thức ăn, thuốc, đồ uống).
/// Khi sử dụng vật phẩm này, các giá trị sẽ được cộng vào stat của player.
/// </summary>
[System.Serializable]
public class Consumable_CompProperties : CompProperties
{
    /// <summary>
    /// Số lượng HP được hồi phục khi sử dụng.
    /// VD: healAmount = 20 sẽ hồi 20 HP.
    /// </summary>
    public float healAmount = 0f;

    /// <summary>
    /// Số lượng đói được thỏa mãn khi sử dụng.
    /// VD: hungerAmount = 30 sẽ giảm 30 điểm đói (tức là thỏa mãn 30).
    /// Giá trị dương = thỏa mãn đói.
    /// </summary>
    public float hungerAmount = 0f;

    /// <summary>
    /// Số lượng khát được thỏa mãn khi sử dụng.
    /// VD: thirstAmount = 25 sẽ giảm 25 điểm khát.
    /// Giá trị dương = thỏa mãn khát.
    /// </summary>
    public float thirstAmount = 0f;

    /// <summary>
    /// Thời gian hiệu lực của buff/debuff (giây).
    /// = 0: hiệu lực ngay lập tức và vĩnh viễn.
    /// > 0: hiệu lực tạm thời trong khoảng thời gian này.
    /// </summary>
    public float duration = 0f;

    /// <summary>
    /// ID của hiệu ứng đặc biệt được kích hoạt (nếu có).
    /// VD: "poison", "speed_boost", "stamina_regen".
    /// Để trống nếu không có hiệu ứng đặc biệt.
    /// </summary>
    public string effectId = "";
}
