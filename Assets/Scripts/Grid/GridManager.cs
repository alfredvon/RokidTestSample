using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    Tile[,] grid;
    [SerializeField] int length = 25;
    [SerializeField] int width = 25;
    [SerializeField] float cellSize = 1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] Unit selectedUnit;

    public Tile GetTile(Vector3 worldPosition)
    {
        worldPosition -= transform.position;
        int x = (int)(worldPosition.x / cellSize);
        int y = (int)(worldPosition.z / cellSize);
        if (x < 0 || x > length || y < 0 || y > length)
            return null;
        return grid[x, y];
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        grid = new Tile[length, width];
        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < length; ++x)
            {
                Tile tile = new Tile();
                tile.SetPos(x, y);
                Vector3 tilePos = GetTileWorldPosition(x, y);
                bool passable = !Physics.CheckBox(tilePos, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer);
                tile.Passable = passable;
                tile.WorldPosition = tilePos;
                grid[x, y] = tile;
                
            }
        }
    }

    Vector3 GetTileWorldPosition(int x, int y)
    {
        return new Vector3(transform.position.x + (x * cellSize), 0f, transform.position.z + (y * cellSize));
    }

    private void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < length; ++x)
            {
                Vector3 pos = GetTileWorldPosition(x, y);
                Gizmos.color = grid[x, y].Passable ? Color.white : Color.red;
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0, cellSize));
            }
        }
    }

}
