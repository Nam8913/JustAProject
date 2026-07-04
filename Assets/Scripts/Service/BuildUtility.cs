using System.Collections.Generic;
using UnityEngine;

public static class BuildUtility
{
    private static BuildMode currentBuildMode = BuildMode.Single;
    private static bool isBuildMode = false;
    private static bool isDragging = false;

    private static Define currentSelectedStructure;
    private static Vector2 dragStartPosition;
    private static Vector2 dragEndPosition;

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
#if UNITY_EDITOR
        Debug.Log("Entered Build Mode");
#endif
    }

    public static void ExitBuildMode()
    {
        isBuildMode = false;
        isDragging = false;
#if UNITY_EDITOR
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
        //TODO: Implement proper structure placement logic based on the selected structure's data
        // For now, we just spawn a placeholder from the pool
        if (currentSelectedStructure == null) return null;
        

        GameObject structureObj = PoolManager.Instance.GetFromPool("Structure",HolderManager.GetHolderObject(HolderManager.StructureHolderName).transform);
        if (structureObj != null)
        {
            structureObj.transform.position = new Vector3(position.x, position.y, 0);
            // Configure the structure object with the selected structure's data
            SpriteRenderer renderer = structureObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                // TODO: Load proper sprite from recipe/define data
                renderer.color = Color.white;
            }


            BoxCollider2D collider2D = structureObj.AddComponent<BoxCollider2D>();
            collider2D.isTrigger = false;
        }
        return structureObj;
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
        return IsBuildableAtTile(position);
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