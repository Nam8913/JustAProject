// ============================================================================
// MethodDef — Định nghĩa MỘT PHƯƠNG THỨC thực hiện hành động (how)
// ============================================================================
//
// MethodDef là "how" — nó mô tả CÁCH thực hiện một hoặc nhiều action.
// Ví dụ: "Chop_Axe" là phương thức chặt bằng rìu, có thể phục vụ action
// "FellTree". "Burn_Fire" là phương thức đốt, có thể phục vụ CẢ "FellTree"
// LẪN "CookFood".
//
// ---- Các field ----
//
// executor (BaseMethodExecutor)
//   Logic thực thi cụ thể. Dùng [SerializeReference] cho polymorphism.
//   Ví dụ: ChopExecutor, BurnExecutor, MachineExecutor
//
// producesEffectTags (List<string>)
//   Danh sách effectTag mà method này có thể tạo ra.
//   Resolver dùng field này để match method với action.
//   Ví dụ: ["TreeFelled"] cho Chop_Axe, ["TreeFelled","FoodCooked"] cho Burn_Fire
//
// requiredActorCapabilities (List<ICondition>)
//   Điều kiện ACTOR phải thỏa để dùng method này.
//   Dùng cùng ICondition như targetRequirements, nhưng check trên Actor.
//   Ví dụ: actor phải có tag "HasAxeEquipped", hoặc "NearFireSource"
//
// parameters (List<EffectMethodParam>)
//   Tham số cho executor (duration, toolWearRate, damage, v.v.)
//   Dạng key-value string, executor tự parse.
//
// extraEffects (List<EffectDef>)
//   Hiệu ứng phụ riêng của method, scoped bởi appliesTo.
//   Ví dụ: Burn_Fire có extraEffect "ProduceItem" ash (áp cho TreeFelled)
//          và "ProduceItem" cooked_meat (áp cho FoodCooked).
//   Nếu appliesTo = null, extraEffect áp cho TẤT CẢ action dùng method này.
//
// ---- Ví dụ XML ----
//
//   <Method id="Chop_Axe" producesEffectTags="TreeFelled" executor="ChopExecutor">
//     <RequiredActorCapabilities>
//       <li Class="HasTagCondition"><tag>HasAxeEquipped</tag></li>
//     </RequiredActorCapabilities>
//     <Parameters>
//       <li><key>duration</key><value>5.0</value></li>
//       <li><key>toolWearRate</key><value>0.1</value></li>
//     </Parameters>
//     <ExtraEffects>
//       <li>
//         <effectType>ProduceItem</effectType>
//         <Parameters><li><key>item</key><value>wood_log</value></li>
//                     <li><key>amount</key><value>3</value></li></Parameters>
//         <appliesTo>TreeFelled</appliesTo>
//       </li>
//     </ExtraEffects>
//   </Method>
//
//   <Method id="Burn_Fire" producesEffectTags="TreeFelled,FoodCooked" executor="BurnExecutor">
//     ...extraEffects có appliesTo khác nhau cho TreeFelled vs FoodCooked...
//   </Method>
//
// ---- Flow liên quan ----
//
//   ActionResolver.ResolveValidMethods(action, allMethods, ctx)
//     → lọc method nào producesEffectTags chứa action.effectTag
//     → filter tiếp theo requiredActorCapabilities
//
//   ActionPipeline.Perform(action, method, ctx)
//     → gọi method.executor.Execute(ctx, method)
//     → sau đó áp extraEffects.Where(appliesTo == action.effectTag)
//
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MethodDef : BuildableData
{
    // Logic thực thi cụ thể (ChopExecutor, BurnExecutor, v.v.)
    [SerializeReference] public BaseMethodExecutor executor;

    // Method này phục vụ những effectTag nào (Resolver match dựa vào đây)
    public List<string> producesEffectTags = new();

    // Điều kiện actor phải thỏa (HasAxeEquipped, NearFireSource, v.v.)
    [SerializeReference] public List<ICondition> requiredActorCapabilities = new();

    // Tham số cho executor (duration, damage, v.v.)
    public List<EffectMethodParam> parameters = new();

    // Hiệu ứng phụ riêng của method, scoped bởi appliesTo
    public List<EffectDef> extraEffects = new();
}
