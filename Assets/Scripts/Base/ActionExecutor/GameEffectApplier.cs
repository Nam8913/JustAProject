// ============================================================================
// GameEffectApplier — Xử lý hiệu ứng chung cho từng effectTag
// ============================================================================
//
// Implement IEffectApplier. Pipeline gọi ApplyEffect(effectTag, ctx, result)
// ở Step 4. Class này translate effectTag thành world-state changes.
//
// ---- Xử lý hiện tại ----
//
//   "TreeFelled"  → target.SetState("Stump"), +wood_log vào actor inventory
//   "FoodCooked"  → target.SetState("Cooked")
//
// ---- Mở rộng ----
//
//   Thêm case mới trong switch để handle effectTag mới.
//   Hoặc tách thành nhiều IEffectApplier nếu game lớn.
//
// ---- Sử dụng ----
//
//   var applier = new GameEffectApplier();
//   var pipeline = new ActionPipeline(resolver, applier);
//
// ============================================================================

public class GameEffectApplier : IEffectApplier
{
    public void ApplyEffect(string effectTag, ActionContext ctx, ActionResult result)
    {
        switch (effectTag)
        {
            case "TreeFelled":
                ApplyTreeFelled(ctx, result);
                break;

            case "FoodCooked":
                ApplyFoodCooked(ctx, result);
                break;

            // Thêm case mới ở đây khi có effectTag mới
        }
    }

    private void ApplyTreeFelled(ActionContext ctx, ActionResult result)
    {
        // Đổi trạng thái target: Standing → Stump
        var state = ctx.Target.GetComp<StateComp>();
        if (state != null)
        {
            state.SetState("Stump");
            result.Consequences.Add(new ConsequenceRecord
            {
                effectTag = "TreeFelled→Stump"
            });
        }
    }

    private void ApplyFoodCooked(ActionContext ctx, ActionResult result)
    {
        // Đổi trạng thái target: Raw → Cooked
        var state = ctx.Target.GetComp<StateComp>();
        if (state != null)
        {
            state.SetState("Cooked");
            result.Consequences.Add(new ConsequenceRecord
            {
                effectTag = "FoodCooked"
            });
        }
    }
}
