using System;

/// <summary>
/// Base class cho tất cả CompProperties.
/// Mỗi CompProperties chứa dữ liệu cấu hình cho một loại EntitiesComp cụ thể.
/// 
/// Cách sử dụng trong XML:
/// <code>
/// <compsProps>
///     <li Class="Weapon_CompProperties">
///         <damage>25</damage>
///         <attackSpeed>1.2</attackSpeed>
///     </li>
/// </compsProps>
/// </code>
/// 
/// Lưu ý: Class attribute phải khớp chính xác tên class trong assembly.
/// </summary>
[System.Serializable]
public class CompProperties
{
    /// <summary>
    /// Kiểu của EntitiesComp mà properties này cấu hình.
    /// Được sử dụng khi cần reference đến comp class cụ thể.
    /// 
    /// VD: compClass = typeof(ProvideContainer_Comp)
    /// 
    /// Lưu ý: Một số CompProperties tự ánh xạ với comp (như ContainerProperties → ProvideContainer_Comp).
    /// </summary>
    public Type compClass; //= typeof(EntitiesComp);
}
