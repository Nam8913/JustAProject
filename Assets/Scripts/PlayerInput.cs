using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerInput
{
    private static PlayerInputActions InputActions => GameService.PlayerInput;
    public static Vector2 Move => InputActions.Player.Move.ReadValue<Vector2>();
    public static Vector2 Look => InputActions.Player.Look.ReadValue<Vector2>();
    public static Vector2 UIMousePosition => InputActions.UI.Point.ReadValue<Vector2>();
    public static Vector2 MousePosition
    {
        get
        {
            Vector2 worldMousePos = GameService.MainCamera.ScreenToWorldPoint(UIMousePosition);
            return worldMousePos;
        }
    }
    public static bool isButtonPressed(string buttonName)
    {
        var button = InputActions.Player.GetType().GetProperty(buttonName)?.GetValue(InputActions.Player) as InputAction;
        if (button != null)
        {
            return button.WasPressedThisFrame();
        }
        else
        {
            Debug.LogError($"Button '{buttonName}' not found in PlayerInputActions.");
            return false;
        }
    }

    public static bool isButtonReleased(string buttonName)
    {
        var button = InputActions.Player.GetType().GetProperty(buttonName)?.GetValue(InputActions.Player) as InputAction;
        if (button != null)
        {
            return button.WasReleasedThisFrame();
        }
        else
        {
            Debug.LogError($"Button '{buttonName}' not found in PlayerInputActions.");
            return false;
        }
    }

    public static bool isButtonDown(string buttonName)
    {
        var button = InputActions.Player.GetType().GetProperty(buttonName)?.GetValue(InputActions.Player) as InputAction;
        if (button != null)
        {
            return button.IsPressed();
        }
        else
        {
            Debug.LogError($"Button '{buttonName}' not found in PlayerInputActions.");
            return false;
        }
    }

    public static bool isMouseWasPressedThisFrame(int buttonIndex)
    {
        switch(buttonIndex)
        {
            case 0:
                return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            case 1:
                return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            case 2:
                return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
            default:
                Debug.LogError($"Invalid mouse button index: {buttonIndex}. Valid indices are 0 (left), 1 (right), and 2 (middle).");
                return false;
        }
    }

    public static bool isMouseWasReleasedThisFrame(int buttonIndex)
    {
        switch(buttonIndex)
        {
            case 0:
                return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
            case 1:
                return Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame;
            case 2:
                return Mouse.current != null && Mouse.current.middleButton.wasReleasedThisFrame;
            default:
                Debug.LogError($"Invalid mouse button index: {buttonIndex}. Valid indices are 0 (left), 1 (right), and 2 (middle).");
                return false;
        }
    }

    public static bool isMouseIsPressed(int buttonIndex)
    {
        switch(buttonIndex)
        {
            case 0:
                return Mouse.current != null && Mouse.current.leftButton.isPressed;
            case 1:
                return Mouse.current != null && Mouse.current.rightButton.isPressed;
            case 2:
                return Mouse.current != null && Mouse.current.middleButton.isPressed;
            default:
                Debug.LogError($"Invalid mouse button index: {buttonIndex}. Valid indices are 0 (left), 1 (right), and 2 (middle).");
                return false;
        }
    }

    public static bool IsMovePressed()
    {
        return Move != Vector2.zero;
    }
}
