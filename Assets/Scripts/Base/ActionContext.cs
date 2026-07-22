// ============================================================================
// ActionContext — Bối cảnh thực thi: AI đang làm gì, với cái gì
// ============================================================================
//
// Truyền xuyên suốt pipeline: Resolver check capabilities trên Actor,
// check targetRequirements trên Target. Executor dùng cả hai.
//
// ---- Các field ----
//
// Actor (DefineThing)
//   Người/thực thể thực hiện hành động.
//   Resolver dùng để check requiredActorCapabilities.
//   Ví dụ: player, NPC, robot
//
// Target (DefineThing)
//   Đối tượng bị tác động.
//   Resolver dùng để check targetRequirements.
//   Ví dụ: cây sồi, cục thịt sống, tờ giấy
//
// ============================================================================

public struct ActionContext
{
    public DefineThing Actor;
    public DefineThing Target;
}
