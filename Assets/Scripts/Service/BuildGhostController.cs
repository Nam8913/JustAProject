using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the ghost preview object during build mode.
/// Handles visual feedback (green = valid, red = invalid) and drag preview for shapes.
/// </summary>
public class BuildGhostController : MonoBehaviour
{
    [Header("Ghost Settings")]
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private Color dragValidColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color dragInvalidColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private string ghostSortingLayer = "Ghost";
    [SerializeField] private int ghostSortingOrder = 10;

    private GameObject _ghostObject;
    private SpriteRenderer _ghostRenderer;
    private List<GameObject> _dragGhosts = new List<GameObject>();
    private Camera _mainCamera;
    private bool _isActive = false;

    readonly string tagPool = "ghostRenderer";

    public bool IsActive => _isActive;

    private void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            _mainCamera = GameService.MainCamera;
        }
    }

    void Start()
    {
        PoolManager.CreateNewPool(tagPool, CreateGhostRenderer(), 50);
    }

    private void Update()
    {
        if (!_isActive || _mainCamera == null) return;

        Vector2 mouseWorldPos = GetMouseWorldPosition();
        if (mouseWorldPos == Vector2.zero) return;

        if (BuildUtility.IsDragging)
        {
            // Update drag end position
            BuildUtility.UpdateDrag(mouseWorldPos);
            UpdateDragGhosts();
        }
        else
        {
            // Update single ghost position
            UpdateSingleGhost(mouseWorldPos);
        }
    }

    /// <summary>
    /// Activate the ghost preview system.
    /// </summary>
    public void ActivateGhost()
    {
        if (_isActive) return;
        _isActive = true;

        // Create or get the main ghost object
        if (_ghostObject == null)
        {
            _ghostObject = PoolManager.Instance.SpawnFromPool(tagPool, Vector3.zero, Quaternion.identity);

            _ghostRenderer = _ghostObject.GetComponent<SpriteRenderer>();
        }

        // Set ghost sprite from selected structure
        Define selected = BuildUtility.GetSelectedStructure();
        if (selected != null)
        {
            // TODO: Load proper sprite from define data
        }

        _ghostObject.SetActive(true);
        Debug.Log("BuildGhostController activated.");
    }

    /// <summary>
    /// Deactivate the ghost preview system and clean up.
    /// </summary>
    public void DeactivateGhost()
    {
        if (!_isActive) return;
        _isActive = false;

        // Hide main ghost
        if (_ghostObject != null)
        {
            _ghostObject.SetActive(false);
        }

        PoolManager.Instance.ReturnToPool(tagPool, _ghostObject);
        _ghostObject = null;
        _ghostRenderer = null;

        // Clear all drag ghosts
        ClearDragGhosts();

        Debug.Log("BuildGhostController deactivated.");
    }

    /// <summary>
    /// Called when drag starts (left mouse button down in Rectangle/Circle mode).
    /// </summary>
    public void OnDragStart()
    {
        if (!_isActive) return;

        Vector2 mousePos = GetMouseWorldPosition();
        BuildUtility.StartDrag(mousePos);

        // Hide main ghost during drag
        if (_ghostObject != null)
        {
            _ghostObject.SetActive(false);
        }

        // Clear old drag ghosts
        ClearDragGhosts();
    }

    /// <summary>
    /// Called when drag ends (left mouse button up).
    /// </summary>
    public void OnDragEnd()
    {
        if (!_isActive) return;

        BuildUtility.EndDrag();
        ClearDragGhosts();

        // Show main ghost again
        if (_ghostObject != null)
        {
            _ghostObject.SetActive(true);
        }
    }

    /// <summary>
    /// Called when drag is cancelled (right click, ESC, etc.).
    /// </summary>
    public void OnDragCancel()
    {
        if (!_isActive) return;

        BuildUtility.CancelDrag();
        ClearDragGhosts();

        // Show main ghost again
        if (_ghostObject != null)
        {
            _ghostObject.SetActive(true);
        }
    }

    /// <summary>
    /// Place the structure at the current position(s).
    /// </summary>
    public void PlaceCurrentStructure()
    {
        if (!_isActive) return;

        if (BuildUtility.IsDragging)
        {
            // If dragging, place all positions in the shape
            List<GameObject> placed = BuildUtility.PlaceAllStructures();
            Debug.Log($"Placed {placed.Count} structures.");
            OnDragEnd();
        }
        else
        {
            // Single placement
            Vector2 mousePos = GetMouseWorldPosition();
            Vector2 placePos = BuildUtility.CurrentBuildMode == BuildUtility.BuildMode.Free
                ? mousePos
                : BuildUtility.SnapToGrid(mousePos);

            if (BuildUtility.IsPlacementValid(placePos))
            {
                GameObject placedStructure = BuildUtility.PlaceStructure(placePos);
                Debug.Log($"Placed structure at {placePos}");
            }
            else
            {
                Debug.LogWarning("Cannot place structure at invalid position.");
            }
        }
    }

    /// <summary>
    /// Cancel build mode entirely.
    /// </summary>
    public void CancelBuildMode()
    {
        if (!_isActive) return;

        OnDragCancel();
        DeactivateGhost();
        BuildUtility.ExitBuildMode();
    }

    private void UpdateSingleGhost(Vector2 mousePosition)
    {
        if (_ghostObject == null || _ghostRenderer == null) return;

        Vector2 ghostPos;
        bool isValid;

        if (BuildUtility.CurrentBuildMode == BuildUtility.BuildMode.Free)
        {
            ghostPos = mousePosition;
            isValid = BuildUtility.IsPlacementValid(ghostPos);
        }
        else
        {
            ghostPos = BuildUtility.SnapToGrid(mousePosition);
            isValid = BuildUtility.IsPlacementValid(ghostPos);
        }

        _ghostObject.transform.position = new Vector3(ghostPos.x, ghostPos.y, 0);
        _ghostRenderer.color = isValid ? validColor : invalidColor;
    }

    private void UpdateDragGhosts()
    {
        // Clear old ghosts
        ClearDragGhosts();

        // Get all positions for the current shape
        Vector2 mousePos = GetMouseWorldPosition();
        List<Vector2> positions = BuildUtility.GetPreviewPositions(mousePos);

        if (positions.Count == 0) return;

        

        // Create ghost for each position
        foreach (Vector2 pos in positions)
        {
            GameObject ghost = PoolManager.Instance.SpawnFromPool(tagPool, Vector3.zero, Quaternion.identity);
            ghost.transform.position = new Vector3(pos.x, pos.y, 0);

            SpriteRenderer renderer = ghost.GetComponent<SpriteRenderer>();
            if(BuildUtility.IsPlacementValid(pos))
            {
                renderer.color = dragValidColor;
            }
            else
            {
                renderer.color = dragInvalidColor;
            }
            renderer.sortingLayerName = ghostSortingLayer;
            renderer.sortingOrder = ghostSortingOrder - 1;

            _dragGhosts.Add(ghost);
        }
    }

    private void ClearDragGhosts()
    {
        foreach (GameObject ghost in _dragGhosts)
        {
            if (ghost != null)
            {
                PoolManager.Instance.ReturnToPool(tagPool, ghost);
            }
        }
        _dragGhosts.Clear();
    }

    private Vector2 GetMouseWorldPosition()
    {
        // if (_mainCamera == null || Mouse.current == null)
        //     return Vector2.zero;

        // Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        // mouseScreenPos.z = -_mainCamera.transform.position.z; // Distance from camera
        // Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);
        // return new Vector2(worldPos.x, worldPos.y);
        return PlayerInput.MousePosition;
    }

    private GameObject CreateGhostRenderer()
    {
        GameObject ghostRendererObj = new GameObject("GhostRenderer");
        ghostRendererObj.transform.position = Vector3.zero;
        ghostRendererObj.transform.rotation = Quaternion.identity;
        ghostRendererObj.transform.localScale = Vector3.one;

        SpriteRenderer render = ghostRendererObj.AddComponent<SpriteRenderer>();
        render.sortingLayerName = ghostSortingLayer;
        render.sortingOrder = ghostSortingOrder;

        // TODO: Load proper sprite from define data
        // For now, create a simple white square texture at runtime
        if (render.sprite == null)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            render.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        ghostRendererObj.SetActive(false);

        return ghostRendererObj;
    }
}