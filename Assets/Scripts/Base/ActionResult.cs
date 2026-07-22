// ============================================================================
// ActionResult — Kết quả trả về sau khi chạy pipeline
// ============================================================================
//
// Chứa status, danh sách lỗi (nếu fail), và danh sách hệ quả (nếu thành công).
//
// ---- Các field ----
//
// Status (ActionStatus)
//   Completed, Failed, InProgress, Canceled, Available
//
// FailedRequirements (List<FailedRequirement>)
//   Danh sách lý do thất bại. Hữu ích cho AI: đọc reason để chọn sub-goal.
//   Ví dụ: "NO_METHOD_AVAILABLE" → "Cần rìu, hoặc lửa, hoặc máy cưa"
//
// Consequences (List<ConsequenceRecord>)
//   Danh sách hệ quả đã xảy ra (dùng cho UI, quest, log).
//   Ví dụ: ["TreeFelled→Stump", "+3 wood_log", "axe durability -0.1"]
//
// ============================================================================

using System.Collections.Generic;

public class ActionResult
{
    public ActionStatus Status;
    public List<FailedRequirement> FailedRequirements = new();
    public List<ConsequenceRecord> Consequences = new();
}
