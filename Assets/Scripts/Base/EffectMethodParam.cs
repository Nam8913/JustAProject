// ============================================================================
// EffectMethodParam — Cặp key-value cho tham số
// ============================================================================
//
// Dùng trong cả MethodDef.parameters (tham số cho executor)
// và EffectDef.parameters (tham số cho effect).
//
// Giá trị là string, executor/effect tự parse sang kiểu phù hợp.
//
// ---- Ví dụ ----
//
//   <li><key>duration</key><value>5.0</value></li>
//   <li><key>item</key><value>wood_log</value></li>
//   <li><key>amount</key><value>3</value></li>
//
// ============================================================================

[System.Serializable]
public class EffectMethodParam
{
    public string key;
    public string value;
}
