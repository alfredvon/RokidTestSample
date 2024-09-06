using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GridController : MonoBehaviour
{
    [SerializeField] GridManager targetGrid;
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] Unit placeUnit;


    PathFinding pathFinding;
    List<Tile> path;
    
    private void Start()
    {
        pathFinding = targetGrid.GetComponent<PathFinding>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                Tile tile = targetGrid.GetTileWithWorldPosition(hit.point);
                tile?.PlaceUnit(placeUnit);
            }
        }
        else if (Input.GetMouseButtonDown(1)) 
        {
            Tile tileStart = targetGrid.GetTileWithWorldPosition(placeUnit.transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                Tile tileEnd = targetGrid.GetTileWithWorldPosition(hit.point);
                if (tileEnd != null && tileStart != null) 
                {
                    path = pathFinding.FindPath(tileStart.Position, tileEnd.Position);
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
