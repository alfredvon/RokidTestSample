using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;

public class GridController : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayerMask;

    GridManager targetGrid;
    GameManager gameManager;

    List<Tile> tempTiles;

    private void Start()
    {
        targetGrid = GridManager.Instance;
        gameManager = GameManager.Instance;
    }



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
                Tile selectTile = targetGrid.GetTileWithWorldPosition(hit.point);
                if (selectTile == null)
                    return;

                gameManager.OnClickTile(selectTile);
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
