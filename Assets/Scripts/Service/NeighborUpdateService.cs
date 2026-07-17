using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Service xử lý neighbor notification khi đặt building mới.
/// Tự động update sprite atlas cho building mới và tất cả neighbors.
/// </summary>
public static class NeighborUpdateService
{
    /// <summary>
    /// Khi đặt building mới tại position, update sprite cho building đó
    /// và tất cả neighbors có hỗ trợ neighbor connection.
    /// </summary>
    public static void OnBuildingPlaced(Vector2Int tilePos, Building newBuilding)
    {
        if (newBuilding == null) return;

        // 1. Tính Direction8 cho building mới
        Direction8 newDir = BuildUtility.GetDirection8ForTile(tilePos);
        newBuilding.NeighborMask = newDir;

        // 2. Update sprite cho building mới
        UpdateBuildingSprite(newBuilding, tilePos);

        // 3. Notify tất cả neighbors
        NotifyNeighbors(tilePos);
    }

    /// <summary>
    /// Khi xóa building, update neighbors.
    /// </summary>
    public static void OnBuildingRemoved(Vector2Int tilePos)
    {
        // Notify tất cả neighbors (building đã bị xóa nên neighbors cần update)
        NotifyNeighbors(tilePos);
    }

    /// <summary>
    /// Notify tất cả neighbors tại 8 hướng xung quanh tilePos.
    /// Luôn update TẤT CẢ neighbors, không check supportsNeighborConnection
    /// vì building đầu tiên đặt trước sẽ có NeighborMask = None.
    /// </summary>
    private static void NotifyNeighbors(Vector2Int tilePos)
    {
        foreach (Direction8 dir in Enum.GetValues(typeof(Direction8)))
        {
            if (dir == Direction8.None || dir == Direction8.All) continue;

            Vector2Int neighborPos = GetNeighborPosition(tilePos, dir);
            Building neighbor = GetBuildingAt(neighborPos);

            // LUÔN update neighbors, không check supportsNeighborConnection
            // vì building đầu tiên đặt trước sẽ có NeighborMask = None
            if (neighbor != null)
            {
                Direction8 neighborDir = BuildUtility.GetDirection8ForTile(neighborPos);
                neighbor.NeighborMask = neighborDir;
                UpdateBuildingSprite(neighbor, neighborPos);
            }
        }
    }

    /// <summary>
    /// Update sprite cho building dựa trên Direction8 hiện tại.
    /// </summary>
    private static void UpdateBuildingSprite(Building building, Vector2Int tilePos)
    {
        if (building == null) return;

        Define define = building.def;
        if (define == null) return;

        SpriteRenderer renderer = building.GetComponent<SpriteRenderer>();
        if (renderer == null) return;

        Sprite sprite = BuildUtility.GetSpriteForDefine(define, new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f));
        if (sprite != null)
        {
            renderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Lấy position của neighbor theo hướng.
    /// </summary>
    private static Vector2Int GetNeighborPosition(Vector2Int pos, Direction8 dir)
    {
        return dir switch
        {
            Direction8.Top => pos + Vector2Int.up,
            Direction8.Bottom => pos + Vector2Int.down,
            Direction8.Left => pos + Vector2Int.left,
            Direction8.Right => pos + Vector2Int.right,
            Direction8.TopLeft => pos + new Vector2Int(-1, 1),
            Direction8.TopRight => pos + new Vector2Int(1, 1),
            Direction8.BottomLeft => pos + new Vector2Int(-1, -1),
            Direction8.BottomRight => pos + new Vector2Int(1, -1),
            _ => pos
        };
    }

    /// <summary>
    /// Lấy Building tại world position.
    /// Dùng OverpPoint thay vì Raycast để reliable hơn.
    /// </summary>
    private static Building GetBuildingAt(Vector2Int tilePos)
    {
        Vector2 worldPos = new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);

        // Dùng OverlapCircle thay vì Raycast(Vector2.zero) - reliable hơn
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null)
        {
            return hit.GetComponent<Building>();
        }
        return null;
    }
}
