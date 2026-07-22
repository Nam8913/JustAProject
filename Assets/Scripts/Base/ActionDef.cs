// ============================================================================
// ActionDef — Định nghĩa MỘT HÀNH ĐỘNG (cái gì thay đổi trong thế giới)
// ============================================================================
//
// ActionDef là "what" — nó KHÔNG BIẾT công cụ nào thực hiện.
// Ví dụ: "FellTree" chỉ nói "cây sẽ bị đốn", không quan tâm bằng rìu hay lửa.
//
// ---- Các field ----
//
// effectTag (string)
//   Định danh hiệu ứng chung mà action này tạo ra.
//   Method nào muốn phục vụ action này phải có effectTag này trong
//   producesEffectTags của nó.
//   Ví dụ: "TreeFelled", "FoodCooked", "DocumentBurned"
//
// targetRequirements (List<ICondition>)
//   Danh sách điều kiện mà TARGET phải thỏa mãn trước khi action được phép.
//   Dùng [SerializeReference] để hỗ trợ polymorphism (HasTagCondition,
//   TargetStateCondition, ...).
//   Ví dụ: target phải có tag "IsTree" VÀ trạng thái "Standing"
//
// OnCompleteEventId (string)
//   Event ID bắn ra sau khi action hoàn thành (dùng cho quest system,
//   trigger chuỗi sự kiện, v.v.).
//   Ví dụ: "tree_felled" → quest "Đốn 5 cây" +1
//
// ---- Ví dụ XML ----
//
//   <Action id="FellTree" effectTag="TreeFelled">
//     <TargetRequirements>
//       <li Class="HasTagCondition"><tag>IsTree</tag></li>
//       <li Class="TargetStateCondition"><requiredState>Standing</requiredState></li>
//     </TargetRequirements>
//     <OnCompleteEventId>tree_felled</OnCompleteEventId>
//   </Action>
//
// ---- Flow liên quan ----
//
//   ActionResolver.CheckTargetRequirements(action, ctx)
//     → duyệt qua targetRequirements, gọi Evaluate(ctx) cho mỗi condition
//
//   ActionPipeline.Perform(action, method, ctx)
//     → dùng effectTag để match với method.producesEffectTags
//     → dùng effectTag để gọi effectApplier.ApplyEffect()
//
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionDef : BuildableData
{
    // Hiệu ứng chung mà action tạo ra (ví dụ: "TreeFelled")
    public string effectTag;

    // Điều kiện target phải thỏa (dùng [SerializeReference] cho polymorphism)
    [SerializeReference] public List<ICondition> targetRequirements = new();

    // Event bắn ra sau khi hoàn thành (quest system, trigger, v.v.)
    public string OnCompleteEventId;
}
