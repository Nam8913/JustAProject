#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
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
        GameObject ghostRendererPrefab = CreateGhostRenderer();
        PoolManager.CreateNewPool(tagPool, ghostRendererPrefab, 50);
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
            //Load proper sprite from define data
            _ghostRenderer.sprite = GetSpriteForSelectedStructure(selected);
        }

        _ghostObject.SetActive(true);
        #if DEBUG_LOG_FLAG && false
        Debug.Log("BuildGhostController activated.");
        #endif
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

        #if DEBUG_LOG_FLAG && false
        Debug.Log("BuildGhostController deactivated.");
        #endif
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
            #if DEBUG_LOG_FLAG && false
            Debug.Log($"Placed {placed.Count} structures.");
            #endif
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
                #if DEBUG_LOG_FLAG && false
                Debug.Log($"Placed structure at {placePos}");
                #endif
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
        return PlayerInput.MousePosition;
    }

    private Sprite GetSpriteForSelectedStructure(Define selected)
    {
        if (selected == null)
        {
            Debug.LogError("Selected structure is null. Cannot get sprite.");
            return null;
        }

        Sprite sprite = null;

        if(selected.graphicData is not null && !string.IsNullOrEmpty(selected.Id))
        {
            if(selected.graphicData is SingleGraphicData singleGraphic)
            {
                sprite = Asset<Sprite>.Get($"{DatabaseThing.GetPackageIdById(selected.Id)}:{singleGraphic.metaData.path}");
                if(sprite == null)
                {
                    Texture2D texture = Asset<Texture2D>.Get($"{DatabaseThing.GetPackageIdById(selected.Id)}:{singleGraphic.metaData.path}");
                    if(texture != null)
                    {
                        sprite = Sprite.Create(texture, new Rect(singleGraphic.metaData.startPos.x, singleGraphic.metaData.startPos.y, singleGraphic.metaData.size.x, singleGraphic.metaData.size.y), singleGraphic.metaData.pivot, singleGraphic.metaData.pixelsPerUnit);
                        if(!Asset<Sprite>.Register($"{DatabaseThing.GetPackageIdById(selected.Id)}:{singleGraphic.metaData.path}", sprite, false))
                        {
                            Debug.LogWarning($"Failed to register sprite for {selected.Id} at path: {singleGraphic.metaData.path}");
                        }
                    }else
                    {
                        Debug.LogError($"Failed to get texture for {selected.Id} at path: {singleGraphic.metaData.path}");
                    }
                }
                return sprite;

            }else if(selected.graphicData is MultiGraphicData multiGraphic)
            {
                // For simplicity, return the first sprite in the list
                if(multiGraphic.metaData != null && multiGraphic.metaData.Count > 0)
                {
                    var firstMeta = multiGraphic.metaData[0];
                    sprite = Asset<Sprite>.Get($"{DatabaseThing.GetPackageIdById(selected.Id)}:{firstMeta.path}");
                    if(sprite == null)
                    {
                        Texture2D texture = Asset<Texture2D>.Get($"{DatabaseThing.GetPackageIdById(selected.Id)}:{firstMeta.path}");
                        if(texture != null)
                        {
                            sprite = Sprite.Create(texture, new Rect(firstMeta.startPos.x, firstMeta.startPos.y, firstMeta.size.x, firstMeta.size.y), firstMeta.pivot, firstMeta.pixelsPerUnit);
                            if(!Asset<Sprite>.Register($"{DatabaseThing.GetPackageIdById(selected.Id)}:{firstMeta.path}", sprite, false))
                            {
                                Debug.LogWarning($"Failed to register sprite for {selected.Id} at path: {firstMeta.path}");
                            }
                        }else
                        {
                            Debug.LogError($"Failed to get texture for {selected.Id} at path: {firstMeta.path}");
                        }
                    }
                    return sprite;
                }
            }else
            {
                Debug.LogError($"GraphicData for {selected.Id} is neither SingleGraphicData nor MultiGraphicData.");
            }
        }

        Debug.LogError(string.Concat(
            $"Failed to get sprite for selected structure with ID: {selected.Id}",
            "\n Is GraphicData null? ", selected.graphicData == null,
            "\n Is GraphicData SingleGraphicData? ", selected.graphicData is SingleGraphicData,
            "\n Is GraphicData MultiGraphicData? ", selected.graphicData is MultiGraphicData,
            "\n Selected structure ID: ", selected.Id,
            "\n Selected structure package ID: ", DatabaseThing.GetPackageIdById(selected.Id),
            "\n Selected structure graphicData: ", selected.graphicData?.ToString() ?? "null"
        ));

        return null;
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

    public void RotateCurrentStructure()
    {
        if (!_isActive) return;

        if(!BuildUtility.IsDragging)
        {
            BuildUtility.RotateCurrentStructure(_ghostObject);
            // TODO: Update ghost sprite if rotation affects it, now we assume the sprite is square and rotation doesn't change its appearance
        }
    }
}