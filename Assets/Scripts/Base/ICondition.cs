// ============================================================================
// ICondition — Interface cho mọi điều kiện kiểm tra
// ============================================================================
//
// Dùng cho CẢ targetRequirements (trên Target) LẪN requiredActorCapabilities
// (trên Actor). Pipeline truyền ActionContext chứa cả hai, condition tự biết
// mình cần check bên nào.
//
// ---- Implementations có sẵn ----
//
// HasTagCondition   — check ctx.Target.HasTag(tag)
// TargetStateCondition — check trạng thái target (dùng tag làm state)
//
// ---- Implement mới ----
//
// Tạo class mới kế thừa ICondition, override Evaluate().
// Dùng [SerializeReference] để XML loader tự resolve type.
//
// ---- Ví dụ XML ----
//
//   <li Class="HasTagCondition"><tag>IsTree</tag></li>
//   <li Class="TargetStateCondition"><requiredState>Standing</requiredState></li>
//
// ============================================================================

[System.Serializable]
public abstract class ICondition
{
    public abstract bool Evaluate(ActionContext ctx);
}
