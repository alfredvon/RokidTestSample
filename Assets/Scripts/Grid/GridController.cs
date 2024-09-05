using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] GridManager targetGrid;
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] Unit placeUnit;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                Tile tile = targetGrid.GetTile(hit.point);
                tile?.PlaceUnit(placeUnit);
            }
        }
    }
}
