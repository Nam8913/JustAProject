#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input for build mode interactions.
/// - Left click: Place structure (Single/Free) or start drag (Rectangle/Circle)
/// - Right click: Cancel build mode or cancel drag
/// - ESC: Cancel build mode
/// - Tracks if mouse is over UI to prevent placement
/// - Syncs BuildGhostController activation with BuildUtility state
/// </summary>
public class HandlePlayerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildGhostController ghostController;

    private Camera _mainCamera;
    private bool _isOverUI = false;
    private bool _wasBuildModeActive = false;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            _mainCamera = GameService.MainCamera;
        }

        // Auto-find ghost controller if not assigned
        if (ghostController == null)
        {
            ghostController = GetComponent<BuildGhostController>();
            if (ghostController == null)
            {
                ghostController = FindAnyObjectByType<BuildGhostController>();
            }
        }
    }

    private void Update()
    {
        // Check if mouse is over UI
        _isOverUI = IsMouseOverUI();

        // Sync ghost controller with build mode state
        SyncBuildModeState();

        // Only process build input if in build mode
        if (!BuildUtility.IsBuildModeActive)
            return;

        HandleBuildInput();
    }

    /// <summary>
    /// Monitors BuildUtility state and activates/deactivates ghost controller accordingly.
    /// </summary>
    private void SyncBuildModeState()
    {
        bool isBuildModeNow = BuildUtility.IsBuildModeActive;

        if (isBuildModeNow && !_wasBuildModeActive)
        {
            // Build mode just became active -> activate ghost
            if (ghostController != null && !ghostController.IsActive)
            {
                ghostController.ActivateGhost();
                #if DEBUG_LOG_FLAG && false
                Debug.Log("HandlePlayerInput: Build mode detected, ghost activated.");
                #endif
            }
        }
        else if (!isBuildModeNow && _wasBuildModeActive)
        {
            // Build mode just became inactive -> deactivate ghost
            if (ghostController != null && ghostController.IsActive)
            {
                ghostController.DeactivateGhost();
                #if DEBUG_LOG_FLAG && false
                Debug.Log("HandlePlayerInput: Build mode ended, ghost deactivated.");
                #endif
            }
        }

        _wasBuildModeActive = isBuildModeNow;
    }

    private void HandleBuildInput()
    {
        // Cancel on ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelBuild();
            return;
        }

        // Cancel on right click
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelBuild();
            return;
        }

        // If clicking UI, cancel drag but don't place
        if (_isOverUI)
        {
            if (BuildUtility.IsDragging)
            {
                CancelDrag();
            }
            return;
        }

        if(Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RotateStructure();
        }

        // Left mouse button handling
        if (Mouse.current != null)
        {
            bool shouldDrag = BuildUtility.CurrentBuildMode == BuildUtility.BuildMode.Rectangle
                           || BuildUtility.CurrentBuildMode == BuildUtility.BuildMode.Circle
                           || BuildUtility.CurrentBuildMode == BuildUtility.BuildMode.Line;

            if (shouldDrag)
            {
                HandleDragInput();
            }
            else
            {
                HandleClickInput();
            }
        }
    }

    private void HandleClickInput()
    {
        // Single/Free mode: place on click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceStructure();
        }
    }

    private void HandleDragInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Start drag
            ghostController.OnDragStart();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // End drag and place
            if (BuildUtility.IsDragging)
            {
                ghostController.PlaceCurrentStructure();
            }
        }
    }

    private void PlaceStructure()
    {
        if (ghostController == null) return;

        // Check if placement position is valid
        Vector2 mousePos = GetMouseWorldPosition();
        Vector2 placePos = BuildUtility.CurrentBuildMode == BuildUtility.BuildMode.Free
            ? mousePos
            : BuildUtility.SnapToGrid(mousePos);

        if (BuildUtility.IsPlacementValid(placePos))
        {
            ghostController.PlaceCurrentStructure();
        }
        else
        {
            Debug.LogWarning("Cannot place structure at invalid position.");
        }
    }

    private void CancelBuild()
    {
        if (ghostController == null) return;

        if (BuildUtility.IsDragging)
        {
            ghostController.OnDragCancel();
        }
        else
        {
            ghostController.CancelBuildMode();
        }
    }

    private void CancelDrag()
    {
        if (ghostController != null)
        {
            ghostController.OnDragCancel();
        }
    }

    private void RotateStructure()
    {
        if (ghostController != null)
        {
            ghostController.RotateCurrentStructure();
        }
    }

    private bool IsMouseOverUI()
    {
        // Use Unity's EventSystem to check if mouse is over UI
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;

        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private Vector2 GetMouseWorldPosition()
    {
        if (_mainCamera == null || Mouse.current == null)
            return Vector2.zero;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = -_mainCamera.transform.position.z;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        return new Vector2(worldPos.x, worldPos.y);
    }
}