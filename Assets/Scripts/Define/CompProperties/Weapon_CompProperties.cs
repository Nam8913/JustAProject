using UnityEngine;

/// <summary>
/// CompProperties cho vũ khí có thể tấn công.
/// Xác định sát thương, tốc độ tấn công, tầm đánh, và các thuộc tính chiến đấu khác.
/// </summary>
[System.Serializable]
public class Weapon_CompProperties : CompProperties
{
    /// <summary>
    /// Sát thương cơ bản mỗi lần tấn công.
    /// VD: damage = 25 sẽ gây 25 sát thương (trước khi tính armor).
    /// </summary>
    public float damage = 10f;

    /// <summary>
    /// Tốc độ tấn công (số lần tấn công/giây).
    /// VD: attackSpeed = 2.0 = 2 lần tấn công mỗi giây.
    /// Càng cao = tấn công càng nhanh.
    /// </summary>
    public float attackSpeed = 1f;

    /// <summary>
    /// Tầm tấn công (đơn vị world).
    /// VD: range = 1.5 = có thể đánh mục tiêu cách 1.5 đơn vị.
    /// melee weapons: 1.0 - 2.5
    /// ranged weapons: 10 - 20
    /// </summary>
    public float range = 1.5f;

    /// <summary>
    /// Loại vũ khí: "melee" (cận chiến) hoặc "ranged" (tầm xa).
    /// Ảnh hưởng đến cách tính damage và animation.
    /// </summary>
    public string weaponType = "melee";

    /// <summary>
    /// Tỷ lệ chí mạng (0.0 - 1.0).
    /// VD: criticalChance = 0.15 = 15% cơ hội chí mạng.
    /// </summary>
    public float criticalChance = 0.05f;

    /// <summary>
    /// Hệ số nhân khi chí mạng.
    /// VD: criticalMultiplier = 2.5 = chí mạng gây 2.5x sát thương.
    /// </summary>
    public float criticalMultiplier = 2f;
}
