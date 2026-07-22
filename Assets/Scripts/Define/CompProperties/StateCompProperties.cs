// ============================================================================
// StateCompProperties — Cấu hình cho StateComp
// ============================================================================
//
// Xác định trạng thái ban đầu của entity.
// Dùng trong XML:
//
//   <compsProps>
//     <li Class="StateCompProperties">
//       <initialState>Standing</initialState>
//     </li>
//   </compsProps>
//
// ============================================================================

[System.Serializable]
public class StateCompProperties : CompProperties
{
    // Trạng thái ban đầu khi entity spawn
    public string initialState = "Default";
}
