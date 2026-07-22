// ============================================================================
// ActionPipeline — Orchestrate toàn bộ flow: resolve → execute → apply effects
// ============================================================================
//
// Pipeline là "nhạc trưởng": điều phối thứ tự các bước khi player thực hiện
// một hành động. KHÔNG chứa logic game cụ thể — chỉ orchestrate.
//
// ---- Flow chi tiết (5 bước) ----
//
//   Step 1: CheckTargetRequirements
//     → Target có thỏa điều kiện không? (ví dụ: target có phải là cây standing?)
//     → Nếu fail → trả ActionResult.Failed + reason
//
//   Step 2: Verify method.producesEffectTags chứa action.effectTag
//     → Method này có phục vụ action này không?
//     → Nếu fail → trả ActionResult.Failed + reason
//
//   Step 3: method.executor.Execute(ctx, method)
//     → Chạy logic cụ thể (animation, timer, v.v.)
//     → Nếu fail → trả methodResult (executor tự quyết định lý do)
//
//   Step 4: effectApplier.ApplyEffect(action.effectTag, ctx, result)
//     → Áp hiệu ứng CHUNG cho effectTag (game layer implement).
//     → Ví dụ: "TreeFelled" → target.SetState(Stump)
//     → Đây là phần mà MỌI method dùng chung khi phục vụ action này.
//
//   Step 5: Áp extraEffects RIÊNG của method (scoped bởi appliesTo)
//     → Chỉ áp effect nào có appliesTo == action.effectTag (hoặc null)
//     → Ví dụ: Chop_Axe → +3 wood_log, -0.1 axe durability
//     → Ví dụ: Burn_Fire + FellTree → +1 ash (không phải cooked_meat)
//
// ---- Ví dụ sử dụng ----
//
//   var resolver = new ActionResolver();
//   var applier = new GameEffectApplier(); // game layer implement
//   var pipeline = new ActionPipeline(resolver, applier);
//
//   var result = pipeline.Perform(fellTreeAction, chopAxeMethod, ctx);
//   // result.Status == Completed
//   // result.Consequences: ["TreeFelled→Stump", "+3 wood_log", "axe -0.1"]
//
// ---- Trace thực tế ----
//
//   > player.TryPerform("FellTree", target: oakTree)
//
//   [Step 1] target.HasTag("IsTree")? ✓  target state Standing? ✓
//   [Step 2] Chop_Axe.producesEffectTags.Contains("TreeFelled")? ✓
//   [Step 3] ChopExecutor.Execute() → 5s animation → Completed
//   [Step 4] GameEffectApplier.ApplyEffect("TreeFelled") → oakTree → Stump
//   [Step 5] extraEffects[0]: ProduceItem wood_log x3
//            extraEffects[1]: DamageTool -0.1
//   [Done]   ActionResult { Completed, Consequences: [...] }
//
// ============================================================================

using System.Collections.Generic;

public class ActionPipeline
{
    private readonly ActionResolver _resolver;
    private readonly IEffectApplier _effectApplier;

    public ActionPipeline(ActionResolver resolver, IEffectApplier effectApplier)
    {
        _resolver = resolver;
        _effectApplier = effectApplier;
    }

    public ActionResult Perform(ActionDef action, MethodDef method, ActionContext ctx)
    {
        var result = new ActionResult();

        // ── Step 1: Target có đủ điều kiện không? ──
        if (!_resolver.CheckTargetRequirements(action, ctx))
        {
            result.Status = ActionStatus.Failed;
            result.FailedRequirements.Add(new FailedRequirement
            {
                reason_output = "Target does not meet requirements for " + action.effectTag
            });
            return result;
        }

        // ── Step 2: Method có phục vụ action này không? ──
        if (!method.producesEffectTags.Contains(action.effectTag))
        {
            result.Status = ActionStatus.Failed;
            result.FailedRequirements.Add(new FailedRequirement
            {
                reason_output = method.label + " cannot produce effect " + action.effectTag
            });
            return result;
        }

        // ── Step 3: Chạy executor (logic cụ thể: animation, timer, v.v.) ──
        var methodResult = method.executor.Execute(ctx, method);
        if (methodResult.Status != ActionStatus.Completed)
        {
            return methodResult;
        }

        // ── Step 4: Áp hiệu ứng CHUNG cho effectTag (game layer implement) ──
        // Ví dụ: "TreeFelled" → target.SetState(Stump)
        _effectApplier.ApplyEffect(action.effectTag, ctx, result);

        // ── Step 5: Áp extraEffects RIÊNG của method (scoped bởi appliesTo) ──
        foreach (var effect in method.extraEffects)
        {
            // Chỉ áp nếu appliesTo match action.effectTag (hoặc null = mọi lúc)
            if (effect.appliesTo != null && effect.appliesTo != action.effectTag)
                continue;

            ApplyExtraEffect(effect, ctx, result);
        }

        // ── Done: Ghi nhận kết quả ──
        result.Status = ActionStatus.Completed;
        result.Consequences.Add(new ConsequenceRecord
        {
            actionId = action.Id,
            methodId = method.Id,
            effectTag = action.effectTag
        });

        return result;
    }

    private void ApplyExtraEffect(EffectDef effect, ActionContext ctx, ActionResult result)
    {
        // Game layer override hoặc extend để interpret effect.effectType
        // Ví dụ: "ProduceItem" → +wood_log, "DamageTool" → -durability
        result.Consequences.Add(new ConsequenceRecord
        {
            effectTag = effect.effectType,
            methodId = effect.effectType
        });
    }
}
