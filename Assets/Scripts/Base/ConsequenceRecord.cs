// ============================================================================
// ConsequenceRecord — Ghi nhận MỘT hệ quả đã xảy ra
// ============================================================================
//
// Ghi lại "cái gì đã xảy ra" sau khi pipeline chạy thành công.
// Dùng cho UI log, quest tracking, AI learning.
//
// ---- Các field ----
//
// actionId (string)
//   ID của Action đã thực hiện. Ví dụ: "FellTree"
//
// methodId (string)
//   ID của Method đã dùng. Ví dụ: "Chop_Axe"
//
// effectTag (string)
//   EffectTag đã apply. Ví dụ: "TreeFelled"
//   Hoặc effectType của extraEffect. Ví dụ: "ProduceItem"
//
// ============================================================================

public struct ConsequenceRecord
{
    public string actionId;
    public string methodId;
    public string effectTag;
}
