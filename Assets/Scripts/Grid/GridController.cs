using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridController : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayerMask;

    List<Tile> tempTiles;



    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //Tile tileStart = selectedUnit.CurrentTile;
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (!isOverUI && Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
            {
                //hitTest = hit.point;
                //Debug.Log(hitTest);
                Tile selectTile = StageManager.Instance.GetGridManager().GetTileWithWorldPosition(hit.point);
                if (selectTile == null)
                    return;

                StageManager.Instance.OnSelectTile(selectTile);
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(hitTest, Vector3.one);
        if (tempTiles == null || tempTiles.Count == 0) return;
        for (int i = 0; i < tempTiles.Count - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(tempTiles[i].WorldPosition, tempTiles[i + 1].WorldPosition);
        }
    }
}
