using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{

    public Material AttackHighlightMaterial;
    public Material MoveHightlightMateiral;
    public Vector2Int Position { get; private set; }
    public bool Passable { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public float Elevation { get; private set; }
    public GameObject QuadObejct;

    [HideInInspector] public Tile parentTile;  // 用于追踪路径

    [HideInInspector] public int gCost;  // 从起点到当前节点的实际代价
    [HideInInspector] public int hCost;  // 从当前节点到目标的预估代价

    [HideInInspector] public int fCost => gCost + hCost;  // fCost = gCost + hCost

    [HideInInspector] public int moveCost = 1;

    public Unit unitOnTile;

    public void SetPos(int x, int y)
    {
        Position = new Vector2Int(x, y);
    }
    public void SetPassable(bool passable)
    {
        Passable = passable;
    }
    public void SetWorldPosition(Vector3 pos)
    {
        WorldPosition = pos;
    }
    public void SetElevation(float elevation)
    {
        Elevation = elevation;
    }

    public bool HasUnit()
    {
        return unitOnTile != null;
    }

    public Unit GetUnit() 
    {
        return unitOnTile;
    }

    public void PlaceUnit(Unit unit, bool sync_posotion = false)
    {
        unitOnTile = unit;
        unitOnTile.SetCurrentTile(this);
        if (sync_posotion)
            unit.gameObject.transform.position = WorldPosition;
    }

    public void RemoveUnit()
    {
        unitOnTile = null;
    }

    public bool IsMovable()
    {
        return Passable && !HasUnit();
    }

    public void ShowMoveHighlight(bool is_highlight)
    {
        if (is_highlight == true)
            QuadObejct.GetComponent<Renderer>().sharedMaterial = MoveHightlightMateiral;
        QuadObejct?.SetActive(is_highlight);
    }

    public void ShowAttackHighlight(bool is_highlight)
    {
        if (is_highlight == true)
            QuadObejct.GetComponent<Renderer>().sharedMaterial = AttackHighlightMaterial;
        QuadObejct?.SetActive(is_highlight);
    }
}
