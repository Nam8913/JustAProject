// ============================================================================
// FailedRequirement — Ghi nhận MỘT lý do thất bại
// ============================================================================
//
// Khi pipeline fail, mỗi FailedRequirement mô tả một lý do.
// AI đọc reason_output để hiểu cần gì → chọn sub-goal rẻ nhất.
//
// ---- Ví dụ ----
//
//   reasonCode: "NO_METHOD_AVAILABLE"
//   reason_output: "Cần rìu, hoặc lửa gần đó, hoặc máy cưa"
//
//   reasonCode: "TARGET_WRONG_STATE"
//   reason_output: "Cây đã bị đốn, không thể đốn lại"
//
// ============================================================================

public struct FailedRequirement
{
    public string reasonCode;
    public string reason_output;
}
