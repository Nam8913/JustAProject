// ============================================================================
// ActorHasTagCondition — Kiểm tra actor có tag cụ thể không
// ============================================================================
//
// Dùng cho requiredActorCapabilities (check trên Actor, KHÔNG phải Target).
// Tương tự HasTagCondition nhưng check ctx.Actor thay vì ctx.Target.
//
// ---- Ví dụ XML ----
//
//   <li Class="ActorHasTagCondition"><tag>HasAxeEquipped</tag></li>
//   → Chỉ cho phép actor có tag "HasAxeEquipped"
//
// ============================================================================

[System.Serializable]
public class ActorHasTagCondition : ICondition
{
    public string tag;

    public override bool Evaluate(ActionContext ctx)
    {
        if (ctx.Actor == null) return false;
        return ctx.Actor.HasTag(tag);
    }
}
