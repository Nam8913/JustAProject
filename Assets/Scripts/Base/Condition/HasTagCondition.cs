// ============================================================================
// HasTagCondition — Kiểm tra target có tag cụ thể không
// ============================================================================
//
// Condition đơn giản nhất: check ctx.Target.HasTag(tag).
// Dùng cho targetRequirements hoặc requiredActorCapabilities.
//
// ---- Ví dụ XML (target requirement) ----
//
//   <li Class="HasTagCondition"><tag>IsTree</tag></li>
//   → Chỉ cho phép target có tag "IsTree"
//
// ---- Ví dụ XML (actor capability) ----
//
//   <li Class="HasTagCondition"><tag>HasAxeEquipped</tag></li>
//   → Chỉ cho phép actor có tag "HasAxeEquipped"
//
// ============================================================================

[System.Serializable]
public class HasTagCondition : ICondition
{
    public string tag;

    public override bool Evaluate(ActionContext ctx)
    {
        if (ctx.Target == null) return false;
        return ctx.Target.HasTag(tag);
    }
}
