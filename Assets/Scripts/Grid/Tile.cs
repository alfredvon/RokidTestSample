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

    [HideInInspector] public Tile parentTile;  // ����׷��·��

    [HideInInspector] public int gCost;  // ����㵽��ǰ�ڵ��ʵ�ʴ���
    [HideInInspector] public int hCost;  // �ӵ�ǰ�ڵ㵽Ŀ���Ԥ������

    [HideInInspector] public int fCost => gCost + hCost;  // fCost = gCost + hCost

    [HideInInspector] public int moveCost = 1;

    public Unit unitOnTile;

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Tile other = (Tile)obj;

        
        return Position == other.Position;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

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

    public void ShowHighlight(TileHighlightType type)
    {

        switch (type)
        {
            case TileHighlightType.Move:
                QuadObejct.GetComponent<Renderer>().sharedMaterial = MoveHightlightMateiral;
                break;
            case TileHighlightType.Ability:
                QuadObejct.GetComponent<Renderer>().sharedMaterial = AttackHighlightMaterial;
                break;
            default:
                break;
        }


        QuadObejct?.SetActive(true);
    }

    public void HideHighlight()
    {
        QuadObejct?.SetActive(false);
    }
}
