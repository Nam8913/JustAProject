// ============================================================================
// EffectDef — Định nghĩa MỘT HIỆU ỨNG PHỤ của Method
// ============================================================================
//
// EffectDef mô tả side-effect mà Method tạo ra khi được sử dụng.
// Điểm quan trọng: field "appliesTo" quyết định effect này chỉ áp dụng
// khi method dùng cho action cụ thể nào.
//
// ---- Ví dụ ----
//
// Burn_Fire có 2 extraEffects:
//   1. effectType="ProduceItem", item=ash,        appliesTo="TreeFelled"
//   2. effectType="ProduceItem", item=cooked_meat, appliesTo="FoodCooked"
//
// Khi Burn_Fire dùng cho FellTree (effectTag="TreeFelled"):
//   → chỉ effect #1 chạy (+ash), effect #2 bị bỏ qua
//
// Khi Burn_Fire dùng cho CookFood (effectTag="FoodCooked"):
//   → chỉ effect #2 chạy (+cooked_meat), effect #1 bị bỏ qua
//
// Nếu appliesTo = null → effect áp cho TẤT CẢ action dùng method này.
//
// ---- Các field ----
//
// effectType (string)
//   Loại hiệu ứng (game layer tự interpret).
//   Ví dụ: "ProduceItem", "DamageTool", "ConsumePower"
//
// parameters (List<EffectMethodParam>)
//   Tham số cho effect (item, amount, v.v.)
//
// appliesTo (string|null)
//   Nếu set, chỉ áp khi method dùng cho action có effectTag matching.
//   Nếu null, áp mọi lúc.
//
// ============================================================================
using System.Collections.Generic;

[System.Serializable]
public class EffectDef
{
    // Loại hiệu ứng (game layer interpret: "ProduceItem", "DamageTool", v.v.)
    public string effectType;

    // Tham số cho effect (item, amount, v.v.)
    public List<EffectMethodParam> parameters = new();

    // Chỉ áp khi method dùng cho action có effectTag này. null = áp mọi lúc.
    public string appliesTo;
}
