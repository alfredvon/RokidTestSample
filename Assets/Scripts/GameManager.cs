using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] GridController gridController;
    [SerializeField] UIManager uiManager;
    [SerializeField] int FirstTurnUnitNum;

    public Unit CurrentSelectedUnit { get; private set; }

    GridManager gridManager;
    PathFinding pathFinding;

    HashSet<int> turnDoneUnitID;
    List<Unit> allUnits;
    int currentActiveIndex;    //当前行动人物在allUnits中的Index
    int turnID;

    List<Tile> tempTiles;
    private void Start()
    {
        if (gridController == null)
            gridController = GetComponent<GridController>();
        gridManager = GridManager.Instance;
        pathFinding = gridManager.pathFinding;
        Init();
    }

    private void Init()
    {
        turnDoneUnitID = new HashSet<int>();
        allUnits = new List<Unit>();
        turnID = 0;
    }

    public void SetCurrentUnit(Unit unit)
    {
        CurrentSelectedUnit = unit;
    }
    
    public bool IsCurrentActiveUnit(int id) 
    {
        return allUnits[currentActiveIndex].GetCharacter().ID == id;
    }

    public List<Unit> GetAllUnitsWithGroup(UnitGroupType group_type)
    {
        List<Unit> units = new List<Unit>();
        foreach (Unit unit in allUnits) 
        {
            if (unit.GetCharacter().Group == group_type)
            {
                units.Add(unit);
            }
        }
        return units;
    }

    public PathFinding GetPathFinding()
    {
        return pathFinding;
    }

    public void UnitStateNotify(Character character)
    {
        if (CurrentSelectedUnit != null && character.ID == CurrentSelectedUnit.GetCharacter().ID)
        {
            if (character.CurrentState == CharacterState.ReadyForAttack)
            {
                SelectUnitOrNot(CurrentSelectedUnit, true);
            }
            else if (character.CurrentState == CharacterState.Attack)
            {
                uiManager.GetCommandPanel().UpdateBtnState(CurrentSelectedUnit.GetCharacter());
                gridManager.ShowAttackableTiles(CurrentSelectedUnit.GetAttackableTiles(), false);
            }
        }

        if (character.CurrentState == CharacterState.TurnDone)
        {
            turnDoneUnitID.Add(character.ID);
            RunTurn();

        }
        else if (character.CurrentState == CharacterState.Generate)
        {
            allUnits.Add(character.ParentUnit);
            if (allUnits.Count == FirstTurnUnitNum)
            {
                NextTurn();
            }
        }
        else if (character.CurrentState == CharacterState.Death)
        {
            
        }
    }

    private void RunTurn()
    {
        
        if (turnDoneUnitID.Count == allUnits.Count)
        {
            NextTurn();
            return;
        }
        currentActiveIndex++;
        Unit current = allUnits[currentActiveIndex];
        if (current.GetCharacter().CurrentState == CharacterState.Death)
        {
            turnDoneUnitID.Add(current.GetCharacter().ID);
            RunTurn();
            return;
        }
        if (current.GetCharacter().IsAI)
        {
            CurrentSelectedUnit = current;
            SelectUnitOrNot(current, true);
            current.GetAI().TakeTurn();
        }
    }

    private void NextTurn()
    { 
        foreach (var unit in allUnits) 
        {
            Character character = unit.GetCharacter();
            character.ResetTurnRes();
        }
        turnID += 1;
        Debug.Log("Turn:" + turnID);
        turnDoneUnitID.Clear();
        // 根据先攻排行动顺序
        allUnits.Sort((a, b) => b.GetCharacter().Initiative.CompareTo(a.GetCharacter().Initiative));
        foreach (var unit in allUnits) { Debug.Log("NextTurn start unit name:" + unit.GetCharacter().Name); }
        currentActiveIndex = 0;
    }

    public void OnClickTile(Tile click_tile)
    {
        Unit currentActiveUnit = allUnits[currentActiveIndex];
        if (currentActiveUnit.GetCharacter().IsAI)
            return;
        Unit selectedUnit = CurrentSelectedUnit;
        if (selectedUnit != null)
        {
            CharacterState cState = selectedUnit.GetCharacter().CurrentState;
            //if could change unit
            if (click_tile.HasUnit())
            {
                bool isSameUnit = selectedUnit.IsEqual(click_tile.GetUnit());
                if (isSameUnit == false 
                    && (cState == CharacterState.Idle || cState == CharacterState.AttackEnd || cState == CharacterState.TurnDone))
                {
                    SelectUnitOrNot(CurrentSelectedUnit, false);
                    CurrentSelectedUnit = click_tile.GetUnit();
                    SelectUnitOrNot(CurrentSelectedUnit, true);
                }
                else
                {
                    SelectUnitOrNot(CurrentSelectedUnit, true);
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

    public void OnClickAttack()
    {
        Unit selectedUnit = CurrentSelectedUnit;
        if (selectedUnit == null || selectedUnit.GetCharacter().TurnAttackDone)
            return;
        CharacterState cState = selectedUnit.GetCharacter().CurrentState;
        if (cState == CharacterState.Move || cState == CharacterState.ReadyForAttack)
            return;
        if (cState == CharacterState.Idle)
            ShowMovableTiles(selectedUnit, false);
        selectedUnit.GetCharacter().SetCharacterState(CharacterState.ReadyForAttack);
    }

    public void OnClickTurnEnd()
    {
        Unit selectedUnit = CurrentSelectedUnit;
        if (selectedUnit == null || selectedUnit.GetCharacter().TurnEnd)
            return;
        SelectUnitOrNot(selectedUnit, false);
        selectedUnit.GetCharacter().DoTurnEnd();
    }

    public void OnClickCancel()
    {
        Unit selectedUnit = CurrentSelectedUnit;
        if (selectedUnit == null)
            return;
        CharacterState cState = selectedUnit.GetCharacter().CurrentState;
        if (cState == CharacterState.ReadyForAttack)
        {
            ShowAttackTiles(selectedUnit, false);
            if (selectedUnit.GetCharacter().TurnMoveDone == false)
            {
                selectedUnit.GetCharacter().SetCharacterState(CharacterState.Idle);
                SelectUnitOrNot(selectedUnit, true);
                return;
            }
        }

        SelectUnitOrNot(CurrentSelectedUnit, false);
        
            
    }

    private void SelectUnitOrNot(Unit unit, bool is_select)
    {
        if (unit == null)
            return;
        //show command panel and update btn show
        if (unit.GetCharacter().IsAI || IsCurrentActiveUnit(unit.GetCharacter().ID) == false)
            uiManager.ShowCommandPanel(false);
        else
            uiManager.ShowCommandPanel(is_select, unit.GetCharacter());
        
        //show or hide tile highlight
        CharacterState characterState = unit.GetCharacter().CurrentState;
        if (characterState == CharacterState.ReadyForAttack)
        {
            ShowAttackTiles(unit, is_select);
        }
        else if (characterState == CharacterState.Idle || characterState == CharacterState.TurnDone)
        {
            ShowMovableTiles(unit, is_select);
        }
        if (is_select == false)
            CurrentSelectedUnit = null;
    }

    void ShowAttackTiles(Unit unit, bool is_select)
    {
        if (is_select == true)
        {
            List<Tile> tiles = pathFinding.GetAttackableTiles(unit.CurrentTile.Position, unit.GetCharacter().AttackRange);
            unit.SetAttackableTiles(tiles);
        }
        gridManager.ShowAttackableTiles(unit.GetAttackableTiles(), is_select);
    }

    void ShowMovableTiles(Unit unit, bool is_select)
    {
        if (is_select == true)
        {
            List<Tile> tiles = pathFinding.GetMovableTiles(unit.CurrentTile.Position, unit.GetCharacter().MovePoints);
            unit.SetMovableTiles(tiles);
        }
        gridManager.ShowMovableTiles(unit.GetMovableTiles(), is_select);
    }

    private void DoUnitActionWithClickTile(Unit unit, Tile tile)
    {
        if (unit == null)
            return;
        if (unit.GetCharacter().IsAI)
            return;
        if (IsCurrentActiveUnit(unit.GetCharacter().ID) == false)
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
                List<Tile> tempTiles = pathFinding.FindPath(tileStart.Position, tile.Position);
                unit.Move(tempTiles);
            }
        }
    }
}
