// ============================================================================
// StateComp — Quản lý trạng thái runtime của entity (per-instance)
// ============================================================================
//
// Mỗi DefineThing có thể có 1 StateComp để track trạng thái hiện tại.
// Đây là per-instance (không chia sẻ như tags trên Define).
//
// ---- Sử dụng ----
//
//   var state = thing.GetComp<StateComp>();
//   state.SetState("Stump");    // đổi trạng thái
//   state.GetState();           // "Stump"
//   state.IsInState("Stump");   // true
//
// ---- Event ----
//
//   state.OnStateChanged += (oldState, newState) => { ... };
//
// ---- XML ----
//
//   <compsProps>
//     <li Class="StateCompProperties">
//       <initialState>Standing</initialState>
//     </li>
//   </compsProps>
//
// ---- GameEffectApplier gọi ----
//
//   effectTag "TreeFelled" → target.SetState("Stump")
//   effectTag "FoodCooked" → target.SetState("Cooked")
//
// ============================================================================

using System;

[System.Serializable]
public class StateComp : EntitiesComp
{
    const string DefaultState = "Default";
    public event Action<string, string> OnStateChanged;

    private string _currentState;

    public override void Init()
    {
        var props = this.props as StateCompProperties;
        _currentState = props?.initialState ?? DefaultState;
        
        if(owner != null && _currentState != DefaultState)
            owner.AddTag(_currentState);
    }

    public string GetState()
    {
        if(_currentState == null)
        {
            UnityEngine.Debug.LogWarning($"StateComp has null state for {this.owner.GetType().Name} with ID: {this.owner.def?.Id}");
        }
        return _currentState;
    }

    public void SetState(string newState)
    {
        if (_currentState == newState) return;

        string oldState = _currentState;
        _currentState = newState;
        OnStateChanged?.Invoke(oldState, newState);
    }

    public bool IsInState(string state)
    {
        return _currentState == state;
    }
}
