using Rokid.UXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GridController gridController;

    public Unit CurrentSelectedUnit { get; private set; }

    GridManager gridManager;
    PathFinding pathFinding;

    List<Tile> tempTiles;
    private void Start()
    {
        if (gridController == null)
            gridController = GetComponent<GridController>();
        gridManager = GridManager.Instance;
        pathFinding = gridManager.pathFinding;
    }

    public void SetCurrentUnit(Unit unit)
    {
        CurrentSelectedUnit = unit;
    }

    public void UnitStateNotify(Unit unit, CharacterState state)
    {
        if (unit.GetCharacter().ID == CurrentSelectedUnit.GetCharacter().ID)
        {
            if (state == CharacterState.ReadyForAttack)
            {
                gridManager.ShowUnitAttackRange(CurrentSelectedUnit);
            }
            else if (state == CharacterState.Attack)
            {
                gridManager.ShowAttackableTiles(CurrentSelectedUnit.GetAttackableTiles(), false);
            }
        }
    }

    public void OnClickTile(Tile click_tile)
    {
        Unit selectedUnit = CurrentSelectedUnit;
        if (selectedUnit != null)
        {
            CharacterState cState = selectedUnit.GetCharacter().CurrentState;
            //if could change unit
            if (click_tile.HasUnit())
            {
                bool isSameUnit = selectedUnit.IsEqual(click_tile.GetUnit());
                if (isSameUnit == false 
                    && (cState == CharacterState.Idle || cState == CharacterState.AttackEnd))
                {
                    SelectUnitOrNot(CurrentSelectedUnit, false);
                    CurrentSelectedUnit = click_tile.GetUnit();
                    SelectUnitOrNot(CurrentSelectedUnit, true);
                }
                else
                {
                    DoUnitActionWithClickTile(CurrentSelectedUnit, click_tile);
                }
            }
            else
            {
                DoUnitActionWithClickTile(CurrentSelectedUnit, click_tile);
            }
        }
        else 
        {
            if (click_tile.HasUnit())
            {
                CurrentSelectedUnit = click_tile.GetUnit();
                SelectUnitOrNot(CurrentSelectedUnit, true);
            }
        }
        
    }

    private void SelectUnitOrNot(Unit unit, bool is_select)
    {
        if (unit == null)
            return;
        CharacterState characterState = unit.GetCharacter().CurrentState;
        if (characterState == CharacterState.ReadyForAttack)
        {
            if (is_select == true)
            {
                List<Tile> tiles = pathFinding.GetAttackableTiles(unit.CurrentTile.Position, unit.GetCharacter().AttackRange);
                unit.SetAttackableTiles(tiles);
            }
            gridManager.ShowAttackableTiles(unit.GetAttackableTiles(), is_select);
        }
        else if (characterState == CharacterState.Idle)
        {
            if (is_select == true)
            {
                List<Tile> tiles = pathFinding.GetMovableTiles(unit.CurrentTile.Position, unit.GetCharacter().MovePoints);
                unit.SetMovableTiles(tiles);
            }
            gridManager.ShowMovableTiles(unit.GetMovableTiles(), is_select);
        }
    }

    private void DoUnitActionWithClickTile(Unit unit, Tile tile)
    {
        if (unit == null)
            return;
        CharacterState characterState = unit.GetCharacter().CurrentState;
        if (characterState == CharacterState.ReadyForAttack)
        {
            if (unit.IsInAttackableTiles(tile))
                unit.Attack(tile);
        }
        else if (characterState == CharacterState.Idle)
        {
            if (tile.IsMovable() && unit.IsMoving == false && unit.IsInMovableTiles(tile))
            {
                Tile tileStart = unit.CurrentTile;
                List<Tile> tempTiles = gridManager.pathFinding.FindPath(tileStart.Position, tile.Position);
                unit.Move(tempTiles);
            }
        }
    }
}
