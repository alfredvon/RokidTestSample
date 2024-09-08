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
            gridManager = GetComponent<GridManager>();
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
                if (!neighbor.IsWalkable() || closedList.Contains(neighbor))
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
    List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parentTile;
        }
        path.Reverse();
        return path;
    }

    // ��ȡ�ھӽڵ�
    List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
         new Vector2Int(-1, 1),  // ����
        new Vector2Int(1, 1),   // ����
        new Vector2Int(-1, -1), // ����
        new Vector2Int(1, -1)   // ����
        };

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
