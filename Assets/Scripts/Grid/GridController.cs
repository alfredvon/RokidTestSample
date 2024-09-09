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
        if (Input.GetMouseButtonDown(0) && selectedUnit != null) 
        {
            Tile tileStart = selectedUnit.CurrentTile;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                Tile tileEnd = targetGrid.GetTileWithWorldPosition(hit.point);
                if (tileEnd != null && tileStart != null) 
                {
                    path = pathFinding.FindPath(tileStart.Position, tileEnd.Position);
                    selectedUnit.Move(path);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (path == null || path.Count == 0) return;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(path[i].WorldPosition, path[i + 1].WorldPosition);
        }
    }
}
