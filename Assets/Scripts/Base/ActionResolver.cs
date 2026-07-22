// ============================================================================
// ActionResolver — Tìm method hợp lệ cho một action + actor
// ============================================================================
//
// Resolver là cầu nối giữa Action (what) và Method (how).
// Nó trả về danh sách method mà actor CÓ THỂ dùng cho action đó.
//
// ---- Flow ----
//
//   1. ResolveValidMethods(action, allMethods, ctx):
//      - Lọc method nào producesEffectTags chứa action.effectTag
//      - Check requiredActorCapabilities trên ctx.Actor
//      - Trả về danh sách method hợp lệ
//
//   2. CheckTargetRequirements(action, ctx):
//      - Duyệt targetRequirements, gọi Evaluate(ctx) cho mỗi condition
//      - Trả false nếu bất kỳ condition nào fail
//
// ---- Ví dụ sử dụng ----
//
//   var resolver = new ActionResolver();
//   var validMethods = resolver.ResolveValidMethods(fellTreeAction, allMethods, ctx);
//
//   if (validMethods.Count == 0) → FAIL: "NO_METHOD_AVAILABLE"
//   if (validMethods.Count == 1) → tự động chọn
//   if (validMethods.Count > 1)  → cho player chọn (hoặc AI tự quyết)
//
// ============================================================================

using System.Collections.Generic;

public class ActionResolver
{
    /// Tìm tất cả method hợp lệ cho action + actor hiện tại.
    /// Hợp lệ = producesEffectTags chứa action.effectTag
    ///        + actor thỏa mọi requiredActorCapabilities
    public List<MethodDef> ResolveValidMethods(ActionDef action, List<MethodDef> allMethods, ActionContext ctx)
    {
        var valid = new List<MethodDef>();

        foreach (var method in allMethods)
        {
            // Method phải hỗ trợ effectTag mà action cần
            if (!method.producesEffectTags.Contains(action.effectTag))
                continue;

            // Actor phải đủ capability
            if (!CheckCapabilities(method.requiredActorCapabilities, ctx))
                continue;

            valid.Add(method);
        }

        return valid;
    }

    /// Kiểm tra target có thỏa mọi điều kiện của action không.
    public bool CheckTargetRequirements(ActionDef action, ActionContext ctx)
    {
        foreach (var condition in action.targetRequirements)
        {
            if (!condition.Evaluate(ctx))
                return false;
        }
        return true;
    }

    /// Kiểm tra actor có thỏa mọi capability không.
    private bool CheckCapabilities(List<ICondition> capabilities, ActionContext ctx)
    {
        foreach (var cap in capabilities)
        {
            if (!cap.Evaluate(ctx))
                return false;
        }
        return true;
    }
}
