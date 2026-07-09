using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CompProperties cho công cụ cung cấp tool qualities.
/// VD: búa cung cấp "tool_hammer", cưa cung cấp "tool_wood_saw".
/// Sử dụng trong recipes để yêu cầu level tool quality nhất định.
/// </summary>
public class ProvideQualities_CompProperties : CompProperties
{
    /// <summary>
    /// Danh sách các tool quality mà vật phẩm này cung cấp.
    /// Mỗi item có thể cung cấp nhiều quality khác nhau.
    /// </summary>
    public List<ProvidedQuality> toolqualities = new List<ProvidedQuality>();

    /// <summary>
    /// Một tool quality được cung cấp bởi vật phẩm.
    /// </summary>
    public class ProvidedQuality
    {
        /// <summary>
        /// ID của tool quality (phải khớp với ToolQualities.xml).
        /// VD: "tool_hammer", "tool_axe", "tool_pickaxe".
        /// </summary>
        public string qualityId;

        /// <summary>
        /// Level của tool quality (bắt đầu từ 1).
        /// VD: level = 3 → thỏa mãn yêu cầu level 1, 2, 3.
        /// Level cao hơn = làm được nhiều việc hơn.
        /// </summary>
        public int level;
    }
}
