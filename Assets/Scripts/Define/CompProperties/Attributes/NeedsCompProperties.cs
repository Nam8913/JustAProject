using System;
using System.Collections.Generic;

/// <summary>
/// CompProperties cho hệ thống nhu cầu sinh tồn (Hunger, Thirst, Sleep, Comfort, Hygiene).
/// 
/// XML usage:
/// <code>
/// <compsProps>
///     <li Class="NeedsCompProperties">
///         <needs>
///             <li>
///                 <name>Hunger</name>
///                 <maxValue>100</maxValue>
///                 <decayRate>0.5</decayRate>
///                 <criticalThreshold>30</criticalThreshold>
///             </li>
///         </needs>
///     </li>
/// </compsProps>
/// </code>
/// 
/// Giá trị need: 100 = đầy đủ, 0 = cực kỳ cần (giảm theo thời gian)
/// </summary>
[Serializable]
public class NeedsCompProperties : CompProperties
{
    /// <summary>
    /// Danh sách các nhu cầu được định nghĩa cho entity này.
    /// </summary>
    public List<NeedData> needs = new List<NeedData>();

    /// <summary>
    /// Dữ liệu cho một nhu cầu cụ thể.
    /// </summary>
    [Serializable]
    public class NeedData
    {
        /// <summary>
        /// Tên nhu cầu: "Hunger", "Thirst", "Sleep", "Comfort", "Hygiene"
        /// </summary>
        public string name = "";

        /// <summary>
        /// Giá trị tối đa (mặc định 100).
        /// 100 = đầy đủ, 0 = cực kỳ cần.
        /// </summary>
        public float maxValue = 100f;

        /// <summary>
        /// Tốc độ giảm mỗi giây (giá trị dương).
        /// VD: decayRate = 0.5 → giảm 0.5/giây
        /// </summary>
        public float decayRate = 0.5f;

        /// <summary>
        /// Ngưỡng cảnh báo (giá trị dưới mức này → cảnh báo).
        /// VD: criticalThreshold = 30 → dưới 30 = cần xử lý
        /// </summary>
        public float criticalThreshold = 30f;

        /// <summary>
        /// Giá trị ban đầu khi spawn (mặc định = maxValue = đầy đủ).
        /// </summary>
        public float startValue = -1f; // -1 = dùng maxValue
    }
}
