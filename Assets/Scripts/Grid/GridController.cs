using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GridController : MonoBehaviour
{
    [SerializeField] GridManager targetGrid;
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] Unit selectedUnit;


    PathFinding pathFinding;
    List<Tile> path;

    //Vector3 hitTest;
    
    private void Start()
    {
        pathFinding = targetGrid.GetComponent<PathFinding>();
        //place unit
        if (selectedUnit != null)
        {
            Tile tile = targetGrid.GetTileWithWorldPosition(selectedUnit.transform.position);
            tile?.PlaceUnit(selectedUnit);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //Tile tileStart = selectedUnit.CurrentTile;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                //hitTest = hit.point;
                //Debug.Log(hitTest);
                Tile selectTile = targetGrid.GetTileWithWorldPosition(hit.point);
                if (selectTile != null) 
                {
                    if (selectTile.HasUnit())
                    {
                        selectedUnit = selectTile.GetUnit();
                    }
                    else if (selectTile.IsWalkable() && selectedUnit != null && selectedUnit.IsMoving == false)
                    {
                        Tile tileStart = selectedUnit.CurrentTile;
                        path = pathFinding.FindPath(tileStart.Position, selectTile.Position);
                        selectedUnit.Move(path);
                    }
                   
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(hitTest, Vector3.one);
        if (path == null || path.Count == 0) return;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(path[i].WorldPosition, path[i + 1].WorldPosition);
        }
    }
}
