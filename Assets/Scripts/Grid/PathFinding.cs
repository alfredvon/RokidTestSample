using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class PathFinding : MonoBehaviour
{
    public GridManager gridManager;
    private void Start()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;
    }

    public List<Tile> GetMovableTiles(Vector2Int start, float move_points)
    {
        Tile startTile = gridManager.GetTileAtPosition(start);
        List<Tile> movableTiles = new List<Tile>();
        Queue<(Tile tile, float remainingMovePoints)> queue = new Queue<(Tile, float)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // ��ʼ�����У�����㿪ʼ
        queue.Enqueue((startTile, move_points));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (currentTile, remainingMove) = queue.Dequeue();

            // �ѵ�ǰ���ƶ��ĸ�����ӵ��б�
            movableTiles.Add(currentTile);

            // �����ĸ�����
            foreach (Tile neighbor in GetNeighbors(currentTile, true))
            {

                // ������ھ���Ч������δ���ʹ�
                if (neighbor.IsMovable() && !visited.Contains(neighbor))
                {
                    int moveCost = neighbor.moveCost;

                    // ���ʣ����ƶ����㹻�ƶ����ø���
                    if (remainingMove >= moveCost)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, remainingMove - moveCost));
                    }
                }
            }
        }

        return movableTiles;
    }

    public List<Tile> GetAttackableTiles(Vector2Int start, int attack_range, bool exclude_self = true)
    {
        List<Tile> attackableTiles = new List<Tile>();

        // ���������ڹ�����Χ�ڵĸ���
        for (int x = -attack_range; x <= attack_range; x++)
        {
            for (int y = -attack_range; y <= attack_range; y++)
            {
                if (exclude_self && x == 0 && y == 0)
                    continue;

                Vector2Int pos = new Vector2Int(start.x + x, start.y + y);

                // ʹ�������پ����ж��Ƿ��ڹ�����Χ��
                if (Mathf.Abs(x) + Mathf.Abs(y) <= attack_range)
                {
                    Tile tile = gridManager.GetTileAtPosition(pos);
                    if (tile == null || !tile.Passable)
                        continue;
                    attackableTiles.Add(tile);
                }
            }
        }

        return attackableTiles;
    }

    public List<Tile> FindPath(Vector2Int start, Vector2Int end)
    {
        Tile startTile = gridManager.GetTileAtPosition(start);
        Tile endTile = gridManager.GetTileAtPosition(end);

        List<Tile> openList = new List<Tile>();
        HashSet<Tile> closedList = new HashSet<Tile>();

        openList.Add(startTile);

        while (openList.Count > 0)
        {
            // ѡ��fCost��С�Ľڵ�
            Tile currentTile = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentTile.fCost || (openList[i].fCost == currentTile.fCost && openList[i].hCost < currentTile.hCost))
                {
                    currentTile = openList[i];
                }
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            // ����ҵ�Ŀ��
            if (currentTile == endTile)
            {
                return RetracePath(startTile, endTile);
            }

            // ������ǰ�ڵ���ھ�
            foreach (Tile neighbor in GetNeighbors(currentTile))
            {
                if (!neighbor.IsMovable() || closedList.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentTile.gCost + GetDistance(currentTile, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endTile);
                    neighbor.parentTile = currentTile;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // δ�ҵ�·��
        return null;
    }

    // �����ҵ���·��
    List<Tile> RetracePath(Tile start_tile, Tile end_tile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = end_tile;

        while (currentTile != start_tile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parentTile;
        }
        path.Reverse();
        return path;
    }

    // ��ȡ�ھӽڵ�
    List<Tile> GetNeighbors(Tile tile, bool use_4_dir = false)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
         new Vector2Int(-1, 1),  // ����
        new Vector2Int(1, 1),   // ����
        new Vector2Int(-1, -1), // ����
        new Vector2Int(1, -1)   // ����
        };
        if (use_4_dir)
        {
            directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        }


        foreach (Vector2Int direction in directions)
        {
            Tile neighbor = gridManager.GetTileAtPosition(tile.Position + direction);
            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    // �������ڵ��ľ���
    int GetDistance(Tile a, Tile b)
    {
        int dstX = Mathf.Abs(a.Position.x - b.Position.x);
        int dstY = Mathf.Abs(a.Position.y - b.Position.y);
        // �Խ��߾��룺ÿ�ζԽ����ƶ��Ĵ���Ϊ 14������ ��2 ԼΪ 1.414 �ļ򻯣�
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);  // ˮƽ�������
        else
            return 14 * dstX + 10 * (dstY - dstX);  // ��ֱ�������
    }
}
