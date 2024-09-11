using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int Position { get; private set; }
    public bool Passable { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public float Elevation { get; private set; }
    public GameObject QuadObejct;

    [HideInInspector] public Tile parentTile;  // ����׷��·��

    [HideInInspector] public int gCost;  // ����㵽��ǰ�ڵ��ʵ�ʴ���
    [HideInInspector] public int hCost;  // �ӵ�ǰ�ڵ㵽Ŀ���Ԥ������

    [HideInInspector] public int fCost => gCost + hCost;  // fCost = gCost + hCost

    [HideInInspector] public int moveCost = 1;

    private Unit unitOnTile;

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

    public void PlaceUnit(Unit unit, bool syncPosotion = false)
    {
        unitOnTile = unit;
        unitOnTile.SetCurrentTile(this);
        if (syncPosotion)
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

    public void ShowHighlight(bool isHighlight)
    {
        QuadObejct?.SetActive(isHighlight);
    }
}
