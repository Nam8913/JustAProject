using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using UnityEngine;

public sealed class Astar
{
    private GraphPathfinder graphPathfinder;
    private List<NodeGraph> path;

    public GraphPathfinder GraphPathfinder
    {
        get => graphPathfinder;
    }

    public List<NodeGraph> Path
    {
        get => path;
    }

    public Astar(GraphPathfinder graphPathfinder)
    {
        this.graphPathfinder = graphPathfinder;

        // Chuẩn bị sẵn dữ liệu graph để có thể tìm đường ngay sau khi khởi tạo.
        Init();
    }

    private void Init()
    {
        // Tạo lưới node 10x10, từ (0,0) đến (9,9).
        for(int x = 0; x < 10; x++)
        {
            for(int y = 0; y < 10; y++)
            {
                // Mỗi node mới được đánh dấu là có thể đi qua.
                graphPathfinder.AddNodeByVector2(new Vector2(x, y), true);
            }
        }

        // Sau khi có node, nối các cạnh theo 8 hướng để tạo đường đi lưới.
        foreach(var node in graphPathfinder.Graph.Values)
        {
            node.FindEdges8Dir(graphPathfinder);
        }
    }

    public List<NodeGraph> FindPath(Vector2 startPos, Vector2 endPos)
    {
        // Tìm node tương ứng với vị trí bắt đầu và kết thúc.
        var startNode = graphPathfinder.GetNode(startPos);
        var endNode = graphPathfinder.GetNode(endPos);

        // Nếu một trong hai node không tồn tại thì không thể tìm đường.
        if (startNode == null || endNode == null)
        {
            Debug.LogError("Start or end node is null!");
            return null;
        }

        // openSet: node đang chờ xét, closedSet: node đã xét xong.
        var openSet = new List<NodeGraph>();
        var closedSet = new HashSet<NodeGraph>();

        // Bắt đầu duyệt từ node xuất phát.
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Chọn node có fCost nhỏ nhất, nếu bằng nhau thì ưu tiên hCost nhỏ hơn.
            var currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Nếu đã tới đích thì dựng lại đường đi và trả kết quả.
            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return path;
            }

            // Duyệt tất cả node kề để cập nhật chi phí đường đi tốt hơn.
            foreach (var edge in currentNode.edges)
            {
                var neighbor = edge.To;
                if (closedSet.Contains(neighbor))
                    continue;

                // Tính chi phí từ start tới neighbor thông qua currentNode.
                float newMovementCostToNeighbor = currentNode.gCost + edge.cost;
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    // hCost là ước lượng khoảng cách còn lại tới đích.
                    neighbor.hCost = Vector2.Distance(neighbor.pos, endNode.pos);
                    // Lưu node cha để có thể truy ngược lại đường đi.
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // Không tìm được đường đi hợp lệ.
        return null;
    }

    private void RetracePath(NodeGraph startNode, NodeGraph endNode)
    {
        // Dựng lại path bằng cách lần ngược từ đích về điểm bắt đầu.
        path = new List<NodeGraph>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // Đảo lại để path đi theo thứ tự từ start -> end.
        path.Reverse();
    }
}