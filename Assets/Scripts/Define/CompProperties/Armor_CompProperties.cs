using UnityEngine;

/// <summary>
/// CompProperties cho giáp/áo giáp.
/// Xác định khả năng phòng thủ, vị trí mặc, độ bền, và trọng lượng.
/// </summary>
[System.Serializable]
public class Armor_CompProperties : CompProperties
{
    /// <summary>
    /// Điểm phòng thủ (giảm sát thương nhận vào).
    /// Công thức giảm: damageReduction = defense / (defense + 100).
    /// VD: defense = 50 → giảm 33% sát thương.
    /// </summary>
    public float defense = 0f;

    /// <summary>
    /// Vị trí mặc giáp: "head", "chest", "legs", "feet", "shield".
    /// Mỗi vị trí chỉ mặc được 1 item.
    /// </summary>
    public string armorSlot = "chest";

    /// <summary>
    /// Độ bền tối đa của giáp.
    /// Giáp sẽ giảm durability mỗi lần nhận sát thương.
    /// Khi durability = 0, giáp không còn hiệu lực.
    /// </summary>
    public float durability = 100f;

    /// <summary>
    /// Trọng lượng giáp (ảnh hưởng đến tốc độ di chuyển nếu có hệ thống encumbrance).
    /// </summary>
    public float weight = 1f;
}
