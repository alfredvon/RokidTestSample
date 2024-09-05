using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public bool Passable { get; set; }
    public Vector2Int Positon { get; private set; }
    public Vector3 WorldPosition { get; set; }

    private Unit unitOnTile;

    public void SetPos(int x, int y)
    { 
        Positon = new Vector2Int(x, y);
    }

    public bool HasUnit()
    {
        return unitOnTile != null;
    }

    public void PlaceUnit(Unit unit)
    {
        unitOnTile = unit;
        unit.gameObject.transform.position = WorldPosition;
    }

    public void RemoveUnit()
    {
        unitOnTile = null;
    }
}
