#if UNITY_EDITOR
#define DEBUG_LOG_FLAG
#endif
using System;
using System.Collections.Generic;
using ModContent;
using UnityEngine;

public static class BuildUtility
{
    private static BuildMode currentBuildMode = BuildMode.Single;
    private static bool isBuildMode = false;
    private static bool isDragging = false;

    private static Define currentSelectedStructure;
    private static Vector2 dragStartPosition;
    private static Vector2 dragEndPosition;
    private static Quaternion currentRotation = Quaternion.identity;

    static BuildUtility()
    {
        isBuildMode = false;
    }

    public static BuildMode CurrentBuildMode => currentBuildMode;
    public static bool IsBuildModeActive => isBuildMode;
    public static bool IsDragging => isDragging;
    public static Vector2 DragStartPosition => dragStartPosition;
    public static Vector2 DragEndPosition => dragEndPosition;
    public static Define SelectedStructure => currentSelectedStructure;

    public static void SetBuildMode(BuildMode mode)
    {
        currentBuildMode = mode;
    }

    public static void SetSelectedStructure(Define structure)
    {
        currentSelectedStructure = structure;
    }

    public static void UnsetSelectedStructure()
    {
        currentSelectedStructure = null;
    }

    public static Define GetSelectedStructure()
    {
        return currentSelectedStructure;
    }

    public static void TriggerBuildMode()
    {
        isBuildMode = true;
        isDragging = false;
        currentRotation = Quaternion.identity; // Reset rotation when entering build mode
#if DEBUG_LOG_FLAG && false
        Debug.Log("Entered Build Mode");
#endif
    }

    public static void ExitBuildMode()
    {
        isBuildMode = false;
        isDragging = false;
        currentRotation = Quaternion.identity; // Reset rotation when exiting build mode
#if DEBUG_LOG_FLAG && false
        Debug.Log("Exited Build Mode");
#endif
        UnsetSelectedStructure();
    }

    public static void StartDrag(Vector2 position)
    {
        if (!isBuildMode) return;
        isDragging = true;
        dragStartPosition = SnapToGrid(position);
        dragEndPosition = dragStartPosition;
    }

    public static void UpdateDrag(Vector2 position)
    {
        if (!isDragging) return;
        dragEndPosition = SnapToGrid(position);
    }

    public static void EndDrag()
    {
        if (!isDragging) return;
        isDragging = false;
    }

    public static void CancelDrag()
    {
        isDragging = false;
    }

    /// <summary>
    /// Get all placement positions based on current build mode and drag state.
    /// </summary>
    public static List<Vector2> GetPlacementPositions()
    {
        if (currentSelectedStructure == null) return new List<Vector2>();

        switch (currentBuildMode)
        {
            case BuildMode.Single:
                return new List<Vector2> { SnapToGrid(dragEndPosition) };

            case BuildMode.Free:
                return new List<Vector2> { dragEndPosition };

            case BuildMode.Line:
                return GetLinePositions(SnapToGrid(dragStartPosition), SnapToGrid(dragEndPosition));

            case BuildMode.Rectangle:
                return GetRectanglePositions(SnapToGrid(dragStartPosition), SnapToGrid(dragEndPosition));

            case BuildMode.Circle:
                return GetCirclePositions(SnapToGrid(dragStartPosition), SnapToGrid(dragEndPosition));

            default:
                return new List<Vector2> { SnapToGrid(dragEndPosition) };
        }
    }

    /// <summary>
    /// Get placement positions for preview (ghost) based on current mode.
    /// For Single/Free: returns 1 position at mouse.
    /// For Line/Rectangle/Circle: returns all positions in the shape.
    /// </summary>
    public static List<Vector2> GetPreviewPositions(Vector2 mousePosition)
    {
        if (currentSelectedStructure == null) return new List<Vector2>();

        switch (currentBuildMode)
        {
            case BuildMode.Single:
                return new List<Vector2> { SnapToGrid(mousePosition) };

            case BuildMode.Free:
                return new List<Vector2> { mousePosition };

            case BuildMode.Line:
                if (isDragging)
                    return GetLinePositions(SnapToGrid(dragStartPosition), SnapToGrid(mousePosition));
                return new List<Vector2> { SnapToGrid(mousePosition) };

            case BuildMode.Rectangle:
                if (isDragging)
                    return GetRectanglePositions(SnapToGrid(dragStartPosition), SnapToGrid(mousePosition));
                return new List<Vector2> { SnapToGrid(mousePosition) };

            case BuildMode.Circle:
                if (isDragging)
                    return GetCirclePositions(SnapToGrid(dragStartPosition), SnapToGrid(mousePosition));
                return new List<Vector2> { SnapToGrid(mousePosition) };

            default:
                return new List<Vector2> { SnapToGrid(mousePosition) };
        }
    }

    /// <summary>
    /// Place a structure at the given position using the object pool.
    /// </summary>
    public static GameObject PlaceStructure(Vector2 position)
    {
        if (currentSelectedStructure == null) return null;

        GameObject structureObj = PoolManager.Instance.GetFromPool("Structure", HolderManager.GetHolderObject(HolderManager.StructureHolderName).transform);
        if (structureObj != null)
        {
            structureObj.transform.position = new Vector3(position.x, position.y, 0);
            structureObj.transform.rotation = currentRotation;

            SpriteRenderer renderer = structureObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white;
            }

            // Thêm collider dựa trên size từ Buildable_CompProperties
            Vector2Int structureSize = GetStructureSize(currentSelectedStructure);
            BoxCollider2D collider2D = structureObj.GetComponent<BoxCollider2D>();
            if (collider2D == null)
                collider2D = structureObj.AddComponent<BoxCollider2D>();

            collider2D.isTrigger = false;
            collider2D.size = new Vector2(structureSize.x, structureSize.y);

            Define define = GetSelectedStructure();
            string modPackageId = DatabaseThing.GetPackageIdById(define.Id);
            Sprite sprite = GetSpriteForDefine(define, position);

            if (sprite == null)
            {
                Debug.LogWarning($"Sprite not found for structure: {define?.name}");
                renderer.sprite = GlobalAssets.GetMissingTexture;
            }
            else
            {
                renderer.sprite = sprite;
            }

            // Thêm Building component nếu chưa có
            Building building = structureObj.GetComponent<Building>();
            if (building == null)
                building = structureObj.AddComponent<Building>();

            building.SetDef(define);

            // Gọi neighbor update
            Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
            NeighborUpdateService.OnBuildingPlaced(tilePos, building);

            Debug.Log($"Placing structure: {define?.name} with sprite: {sprite?.name} at position: {position}");
        }
        return structureObj;
    }

    public static Sprite GetSpriteForDefine(Define define, Vector2 pos)
    {
        if (define == null)
        {
            Debug.LogError("Define is null. Cannot get sprite.");
            return GlobalAssets.GetMissingTexture;
        }

        string modPackageId = DatabaseThing.GetPackageIdById(define.Id);
        ModAssets modAssets = GlobalAssets.GetModAssets(modPackageId);
        Sprite sprite = null;

        if (define.graphicData is SingleGraphicData singleGraphicData)
        {
            //sprite = Asset<Sprite>.Get($"{modPackageId}:{singleGraphicData.metaData.path}");
            modAssets.TryGetAsset(singleGraphicData.metaData.path, out sprite);
        }
        else if (define.graphicData is MultiGraphicData multiGraphicData)
        {
            //sprite = Asset<Sprite>.Get($"{modPackageId}:{multiGraphicData.metaData[0].path}");
            modAssets.TryGetAsset(multiGraphicData.metaData[0].path, out sprite);
        }
        else if (define.graphicData is AtlasGraphicData atlasGraphicData)
        {
            Direction8 dir = BuildUtility.GetDirection8ForTile(pos);
            string spritePath = $"{atlasGraphicData.atlasPath}_{dir.ToString().Replace(", ", "|")}";
            //sprite = Asset<Sprite>.Get(spritePath);
            modAssets.TryGetAsset(spritePath, out sprite);
        }else
        {
            Debug.LogWarning($"GraphicData type not recognized for structure: {define.name} of type {define.graphicData?.GetType().Name}");
        }

        if (sprite == null)
        {
            Debug.LogWarning($"Sprite not found for structure: {define.name}");
            return GlobalAssets.GetMissingTexture;
        }

        return sprite;
    }

    /// <summary>
    /// Place all structures from the current placement positions.
    /// </summary>
    public static List<GameObject> PlaceAllStructures()
    {
        List<GameObject> placed = new List<GameObject>();
        List<Vector2> positions = GetPlacementPositions();
        foreach (Vector2 pos in positions)
        {
            if(!BuildUtility.IsPlacementValid(pos))
            {
                continue; // Skip invalid positions
            }

            GameObject obj = PlaceStructure(pos);
            if (obj != null)
                placed.Add(obj);
        }
        return placed;
    }

    /// <summary>
    /// Check if a position is valid for placement.
    /// </summary>
    public static bool IsPlacementValid(Vector2 position)
    {
        if (currentBuildMode == BuildMode.Free)
            return IsBuildableAtPositionFree(position);

        // Đọc size từ Buildable_CompProperties
        Vector2Int size = GetStructureSize(currentSelectedStructure);

        if (size.x <= 1 && size.y <= 1)
            return IsBuildableAtTile(position);

        // Multi-tile: kiểm tra tất cả tiles
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2 checkPos = new Vector2(position.x + x, position.y + y);
                if (!IsBuildableAtTile(checkPos))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Lấy size từ Buildable_CompProperties trong Define.
    /// </summary>
    public static Vector2Int GetStructureSize(Define define)
    {
        if (define?.compsProps == null) return Vector2Int.one;

        foreach (var comp in define.compsProps)
        {
            if (comp is Buildable_CompProperties buildable)
                return buildable.size;
        }
        return Vector2Int.one;
    }

    /// <summary>
    /// Kiểm tra Define có thể rotate không.
    /// </summary>
    public static bool IsRotatable(Define define)
    {
        if (define?.compsProps == null) return false;

        foreach (var comp in define.compsProps)
        {
            if (comp is Buildable_CompProperties buildable)
                return buildable.rotatable;
        }
        return false;
    }

    private static bool IsBuildableAtPosition(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.05f);
        if (hit.collider != null)
            return false;
        return true;
    }

    private static bool IsBuildableAtPositionFree(Vector3 position)
    {
        // Free mode: just check no overlap at exact position
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.05f);
        if (hit.collider != null)
            return false;
        return true;
    }

    public static bool IsBuildableAtTile(Vector3 pos)
    {
        Vector2 tilePos = new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
        if (!IsBuildableAtPosition(pos))
            return false;

        // Create 4 raycasts to check the corners of the tile
        Vector2[] corners = new Vector2[]
        {
            tilePos + new Vector2(-0.40f, -0.40f),
            tilePos + new Vector2(0.40f, -0.40f),
            tilePos + new Vector2(-0.40f, 0.40f),
            tilePos + new Vector2(0.40f, 0.40f)
        };
        foreach (var corner in corners)
        {
            if (!IsBuildableAtPosition(corner))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Snap a position to the grid (center of tile).
    /// </summary>
    public static Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(Mathf.Floor(position.x) + 0.5f, Mathf.Floor(position.y) + 0.5f);
    }

    public static Direction8 GetDirection4ForTile(Vector2 pos)
    {
        Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        Vector2 centerPos = new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);
        
        Vector2[] directions = new Vector2[]
        {
            new Vector2(0, 1),    // Top
            new Vector2(1, 0),    // Right
            new Vector2(0, -1),   // Bottom
            new Vector2(-1, 0)    // Left
        };
        Direction8 result = Direction8.None;
        foreach(var dir in directions)
        {
            Vector2 checkPos = centerPos + dir;
            if (!IsBuildableAtPosition(checkPos))
            {
                // Determine which Direction8 corresponds to this direction
                if (dir == new Vector2(0, 1)) result |= Direction8.Top;
                if (dir == new Vector2(1, 0)) result |= Direction8.Right;
                if (dir == new Vector2(0, -1)) result |= Direction8.Bottom;
                if (dir == new Vector2(-1, 0)) result |= Direction8.Left;
            }
        }
        return result;
    }

    public static Direction8 GetDirection8ForTile(Vector2 pos)
    {
        Direction8 dir = Direction8.None;

        Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y));
        Vector2 centerPos = new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);

        // ===== Kiểm tra 4 hướng =====
        if (!IsBuildableAtPosition(centerPos + Vector2.up))
            dir |= Direction8.Top;

        if (!IsBuildableAtPosition(centerPos + Vector2.right))
            dir |= Direction8.Right;

        if (!IsBuildableAtPosition(centerPos + Vector2.down))
            dir |= Direction8.Bottom;

        if (!IsBuildableAtPosition(centerPos + Vector2.left))
            dir |= Direction8.Left;

        // ===== Chỉ kiểm tra đường chéo khi đủ 2 cạnh =====

        if ((dir & (Direction8.Top | Direction8.Left)) == (Direction8.Top | Direction8.Left))
        {
            if (!IsBuildableAtPosition(centerPos + new Vector2(-1, 1)))
                dir |= Direction8.TopLeft;
        }

        if ((dir & (Direction8.Top | Direction8.Right)) == (Direction8.Top | Direction8.Right))
        {
            if (!IsBuildableAtPosition(centerPos + new Vector2(1, 1)))
                dir |= Direction8.TopRight;
        }

        if ((dir & (Direction8.Bottom | Direction8.Left)) == (Direction8.Bottom | Direction8.Left))
        {
            if (!IsBuildableAtPosition(centerPos + new Vector2(-1, -1)))
                dir |= Direction8.BottomLeft;
        }

        if ((dir & (Direction8.Bottom | Direction8.Right)) == (Direction8.Bottom | Direction8.Right))
        {
            if (!IsBuildableAtPosition(centerPos + new Vector2(1, -1)))
                dir |= Direction8.BottomRight;
        }

        return dir;
    }

    #region Shape Calculation

    private static List<Vector2> GetLinePositions(Vector2 start, Vector2 end)
    {
        List<Vector2> positions = new List<Vector2>();
        int dx = Mathf.RoundToInt(end.x - start.x);
        int dy = Mathf.RoundToInt(end.y - start.y);
        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

        if (steps == 0)
        {
            positions.Add(start);
            return positions;
        }

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(start, end, t);
            positions.Add(SnapToGrid(point));
        }

        return positions;
    }

    private static List<Vector2> GetRectanglePositions(Vector2 start, Vector2 end)
    {
        List<Vector2> positions = new List<Vector2>();
        int minX = Mathf.RoundToInt(Mathf.Min(start.x, end.x));
        int maxX = Mathf.RoundToInt(Mathf.Max(start.x, end.x));
        int minY = Mathf.RoundToInt(Mathf.Min(start.y, end.y));
        int maxY = Mathf.RoundToInt(Mathf.Max(start.y, end.y));

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                positions.Add(new Vector2(x + 0.5f, y + 0.5f));
            }
        }

        return positions;
    }

    private static List<Vector2> GetCirclePositions(Vector2 center, Vector2 edge)
{
    List<Vector2> positions = new List<Vector2>();
    float radius = Vector2.Distance(center, edge);
    int radiusInt = Mathf.Max(1, Mathf.RoundToInt(radius));

    // Độ dày viền (tính bằng ô). 0.5f ~ viền dày khoảng 1 ô
    float thickness = 0.5f;

    for (int x = -radiusInt; x <= radiusInt; x++)
    {
        for (int y = -radiusInt; y <= radiusInt; y++)
        {
            Vector2 candidate = new Vector2(center.x + x, center.y + y);
            float dist = Vector2.Distance(candidate, center);

            // Chỉ lấy các ô có khoảng cách nằm sát quanh bán kính (viền ngoài)
            if (Mathf.Abs(dist - radius) <= thickness)
            {
                positions.Add(SnapToGrid(candidate));
            }
        }
    }
    return positions;
}

    public static void RotateCurrentStructure(GameObject _ghostObject)
    {
        _ghostObject.transform.Rotate(0, 0, -90); // Rotate the ghost object by -90 degrees
        currentRotation = _ghostObject.transform.rotation; // Store the new rotation
    }

    #endregion

    public enum BuildMode
    {
        Single,
        Free,
        Line,
        Rectangle,
        Circle,
    }
}