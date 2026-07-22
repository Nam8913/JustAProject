// ============================================================================
// IEffectApplier — Interface cho game layer để áp hiệu ứng chung
// ============================================================================
//
// Pipeline gọi effectApplier.ApplyEffect(effectTag, ctx, result) ở Step 4.
// Base layer không biết "TreeFelled" nghĩa là gì — game layer implement
// interface này để handle domain-specific effects.
//
// ---- Ví dụ implement ----
//
//   public class GameEffectApplier : IEffectApplier
//   {
//       public void ApplyEffect(string effectTag, ActionContext ctx, ActionResult result)
//       {
//           switch (effectTag)
//           {
//               case "TreeFelled":
//                   ctx.Target.SetState("Stump");
//                   result.Consequences.Add(new ConsequenceRecord { effectTag = "TreeFelled→Stump" });
//                   break;
//               case "FoodCooked":
//                   ctx.Target.SetState("Cooked");
//                   result.Consequences.Add(new ConsequenceRecord { effectTag = "FoodCooked" });
//                   break;
//           }
//       }
//   }
//
// ============================================================================

public interface IEffectApplier
{
    void ApplyEffect(string effectTag, ActionContext ctx, ActionResult result);
}
