// ============================================================================
// BaseMethodExecutor — Abstract base cho mọi executor thực thi method
// ============================================================================
//
// Executor chứa logic "làm thế nào" để thực hiện method.
// Ví dụ: ChopExecutor chạy animation chặt 5s, BurnExecutor chạy animation đốt 20s.
//
// ---- Implementations có sẵn ----
//
// Attack (MethodExecutor/Attack.cs) — demo executor, log damage
//
// ---- Implement mới ----
//
// Tạo class mới kế thừa BaseMethodExecutor, override Execute().
// Dùng [SerializeReference] trong MethodDef để XML loader tự resolve type.
//
// ---- Ví dụ XML ----
//
//   <Method id="Chop_Axe" executor="ChopExecutor">
//     ...
//   </Method>
//
// ---- Flow ----
//
//   ActionPipeline.Perform() gọi method.executor.Execute(ctx, method)
//   → executor chạy logic riêng (animation, timer, v.v.)
//   → trả ActionResult (Completed / Failed)
//   → nếu Completed, pipeline tiếp tục áp effects
//
// ============================================================================

public abstract class BaseMethodExecutor
{
    public abstract ActionResult Execute(ActionContext ctx, MethodDef method);
}
