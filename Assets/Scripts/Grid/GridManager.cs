using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    Tile[,] grid;
    [SerializeField] int length = 25;
    [SerializeField] int width = 25;
    [SerializeField] float cellSize = 1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask gridLayer;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject tileRoot;


    public Tile GetTileWithWorldPosition(Vector3 worldPosition)
    {
        worldPosition -= transform.position;
        float half = cellSize / 2;
        int x = (int)((worldPosition.x + half) / cellSize);
        int y = (int)((worldPosition.z + half) / cellSize);
        if (x < 0 || x >= length || y < 0 || y >= width)
            return null;
        return grid[x, y];
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= length || y < 0 || y >= width)
            return null;
        return grid[x, y];
    }

    protected override void Awake()
    {
        base.Awake();
        GenerateGrid();
    }

    private void Start()
    {
        //GenerateGrid();
    }

    private void GenerateGrid()
    {
        grid = new Tile[length, width];
        RaycastHit hit;
        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < length; ++x)
            {
                Tile tile = new Tile();
                tile.SetPos(x, y);
                Vector3 tilePos = GetTileWorldPosition(x, y);
                //elevation
                Ray ray = new Ray(tilePos + Vector3.up * 100f, Vector3.down);
                if (Physics.Raycast(ray, out hit, float.MaxValue, gridLayer))
                {
                    tile.SetElevation(hit.point.y);
                    tilePos.y = hit.point.y;
                }
                //passable
                bool passable = !Physics.CheckBox(tilePos, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer);
                tile.SetPassable(passable);
                tile.SetWorldPosition(tilePos);
                GameObject tileObject = Instantiate(tilePrefab, tilePos + tilePrefab.transform.position, tilePrefab.transform.rotation, tileRoot.transform);
                tile.SetQuadObject(tileObject);
                grid[x, y] = tile;
                
            }
        }
    }

    Vector3 GetTileWorldPosition(int x, int y, float elevation = 0f)
    {
        return new Vector3(transform.position.x + (x * cellSize), elevation, transform.position.z + (y * cellSize));
    }

    private void OnDrawGizmos()
    {
        //if (grid == null) return;

        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < length; ++x)
            {
                Vector3 pos = GetTileWorldPosition(x, y);
                if (grid != null)
                {
                    pos = grid[x, y].WorldPosition;
                    Gizmos.color = grid[x, y].Passable ? Color.white : Color.red;
                }

                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0, cellSize));
            }
        }
    }

}
