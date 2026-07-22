// ============================================================================
// DefaultEffectApplier — No-op implementation (base layer placeholder)
// ============================================================================
//
// Game layer override bằng implementation thực sự.
// Nếu không có game layer nào register, effect chung sẽ không xảy ra
// (chỉ extraEffects của method chạy).
//
// ============================================================================

public class DefaultEffectApplier : IEffectApplier
{
    public void ApplyEffect(string effectTag, ActionContext ctx, ActionResult result)
    {
        // No-op. Game layer override để handle domain-specific effects.
    }
}
