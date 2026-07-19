using System;
using System.Collections.Generic;

/// <summary>
/// CompProperties cho thuộc tính cơ bản của Creature (Strength, Agility, Intelligence, etc.).
/// 
/// XML usage:
/// <code>
/// <compsProps>
///     <li Class="AttributesCompProperties">
///         <attributes>
///             <li>
///                 <name>Strength</name>
///                 <baseValue>10</baseValue>
///             </li>
///             <li>
///                 <name>Agility</name>
///                 <baseValue>10</baseValue>
///             </li>
///         </attributes>
///     </li>
/// </compsProps>
/// </code>
/// </summary>
[Serializable]
public class AttributesCompProperties : CompProperties
{
    /// <summary>
    /// Danh sách thuộc tính cơ bản.
    /// </summary>
    public List<AttributeData> attributes = new List<AttributeData>();

    /// <summary>
    /// Dữ liệu cho một thuộc tính cụ thể.
    /// </summary>
    [Serializable]
    public class AttributeData
    {
        /// <summary>
        /// Tên thuộc tính: "Strength", "Agility", "Intelligence", "Perception", "Constitution", "Charisma", "Luck"
        /// </summary>
        public string name = "";

        /// <summary>
        /// Giá trị cơ bản.
        /// </summary>
        public float baseValue = 10f;
    }
}
