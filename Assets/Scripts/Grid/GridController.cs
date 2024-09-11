using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GridController : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] Unit selectedUnit;

    GridManager targetGrid;
    PathFinding pathFinding;
    List<Tile> tempTiles;

    //Vector3 hitTest;

    private void Start()
    {
        targetGrid = GridManager.Instance;
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
                        if (selectedUnit != null)
                        {
                            targetGrid.ShowMovableTiles(selectedUnit.GetMovableTiles(), false);
                        }
                        selectedUnit = selectTile.GetUnit();

                        if (selectedUnit.GetCharacter().CurrentState == CharacterState.ReadyForAttack)
                        {
                            tempTiles = pathFinding.GetAttackableTiles(selectedUnit.CurrentTile.Position, selectedUnit.GetCharacter().AttackRange);
                            selectedUnit.SetAttackableTiles(tempTiles);
                            targetGrid.ShowMovableTiles(tempTiles, true);
                        }
                        else
                        {
                            tempTiles = pathFinding.GetMovableTiles(selectedUnit.CurrentTile.Position, selectedUnit.GetMovePoints());
                            selectedUnit.SetMovableTiles(tempTiles);
                            targetGrid.ShowMovableTiles(tempTiles, true);
                        }
                        
                    }
                    else if (selectedUnit != null)
                    {
                        CharacterState characterState = selectedUnit.GetCharacter().CurrentState;
                        if (characterState == CharacterState.Idle)
                        {
                            if (selectTile.IsMovable() && selectedUnit.IsMoving == false && selectedUnit.IsInMovableTiles(selectTile))
                            {
                                Tile tileStart = selectedUnit.CurrentTile;
                                tempTiles = pathFinding.FindPath(tileStart.Position, selectTile.Position);
                                selectedUnit.Move(tempTiles);
                            }
                        }
                        else if (characterState == CharacterState.ReadyForAttack)
                        {
                            if (selectedUnit.IsInAttackableTiles(selectTile))
                                selectedUnit.Attack(selectTile);
                        }

                    }
                   
                }
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
