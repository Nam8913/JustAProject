// ============================================================================
// TargetStateCondition — Kiểm tra target đang ở trạng thái cụ thể
// ============================================================================
//
// Dùng StateComp (per-instance) thay vì tags (shared Define data).
// StateComp phải được add vào DefineThing trước khi condition này hoạt động.
//
// ---- Ví dụ XML ----
//
//   <li Class="TargetStateCondition"><requiredState>Standing</requiredState></li>
//   → Chỉ cho phép target đang ở trạng thái "Standing"
//
// ============================================================================

[System.Serializable]
public class TargetStateCondition : ICondition
{
    public string requiredState;

    public override bool Evaluate(ActionContext ctx)
    {
        if (ctx.Target == null) return false;
        var state = ctx.Target.GetComp<StateComp>();
        if (state == null) return false;
        return state.IsInState(requiredState);
    }
}
