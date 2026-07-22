// ============================================================================
// ChopExecutor — Logic thực thi chặt cây bằng rìu
// ============================================================================
//
// Kế thừa BaseMethodExecutor. Chạy khi Method "Chop_Axe" được chọn
// cho Action "FellTree".
//
// ---- Flow ----
//
//   1. Kiểm tra actor có rìu không (capability đã check ở Resolver)
//   2. Log "chopping..." (sau này thay bằng animation)
//   3. Trả ActionResult.Completed
//   4. Pipeline sẽ tiếp tục: GameEffectApplier → TreeFelled → Stump
//                        → ExtraEffects → +wood_log, -axe durability
//
// ============================================================================

using UnityEngine;

public class ChopExecutor : BaseMethodExecutor
{
    public override ActionResult Execute(ActionContext ctx, MethodDef method)
    {
        // Lấy duration từ parameters (nếu có)
        float duration = 5f;
        foreach (var p in method.parameters)
        {
            if (p.key == "duration" && float.TryParse(p.value, out float d))
            {
                duration = d;
                break;
            }
        }

        Debug.Log($"[ChopExecutor] {ctx.Actor.LabelName} bắt đầu chặt {ctx.Target.LabelName} ({duration}s)...");

        // TODO: Sau này thay bằng coroutine/animation timer
        // Hiện tại trả Completed ngay (instant)

        Debug.Log("[ChopExecutor] Hoàn tất.");

        return new ActionResult { Status = ActionStatus.Completed };
    }
}
