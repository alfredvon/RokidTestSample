using Rokid.UXR.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] GridController gridController;
    [SerializeField] List<Unit> playerUnits = new List<Unit>();
    [SerializeField] List<Unit> enemyUnits = new List<Unit>();

    GridManager gridManager;
    UIManager uiManager;

    List<Unit> allUnits = new List<Unit>();
    Queue<Unit> activeUnits = new Queue<Unit>();
    HashSet<int> turnDoneUnitIDs = new HashSet<int>();
    Unit curUnit;   //当前行动回合的目标
    Unit selectUnit;    //当前点击的目标
    int turnID;
    bool stageWin = false;
    bool stageEnd = false;

    StageState curState = StageState.None;
    StageWinCondition winCondition = StageWinCondition.DefeatAllEnemies;

    public GridManager GetGridManager() => gridManager;

    public List<Unit> GetUnitsWithGroupType(UnitGroupType group_type)
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

    public void OnClickCommandPanelAttack()
    {
        if (curUnit == null || curUnit.GetCharacter().TurnAttackDone)
            return;
        CharacterState cState = curUnit.GetCharacter().CurrentState;
        if (cState == CharacterState.Move || cState == CharacterState.ReadyForAttack)
            return;
        if (cState == CharacterState.Idle)
            ShowMovableTiles(curUnit, false);
        curUnit.GetCharacter().SetCharacterState(CharacterState.ReadyForAttack);
    }

    public void OnClickCommandPanelTurnEnd()
    {
        if (curUnit == null || curUnit.GetCharacter().TurnEnd == true)
            return;
        OnDeselectUnit();
        curUnit.GetCharacter().DoTurnEnd();
    }

    public void OnClickCommandPanelCancel()
    {
        if (curUnit == null)
            return;
        OnDeselectUnit();
    }

    public void OnSelectTile(Tile select_tile)
    {
        if (curUnit.GetCharacter().IsAI)
            return;
        
        if (selectUnit != null)
        {
            //if change select unit
            if (select_tile.HasUnit())
            {
                CharacterState cState = selectUnit.GetCharacter().CurrentState;
                bool isSameUnit = selectUnit.IsEqual(select_tile.GetUnit());
                if (isSameUnit == false
                    && (cState == CharacterState.Idle || cState == CharacterState.AttackEnd || cState == CharacterState.TurnDone))
                {
                    OnDeselectUnit();
                    OnSelectUnit(select_tile.GetUnit());
                }
                else
                {
                    OnSelectUnit(selectUnit);
                    DoActionBySelectTile(select_tile);
                }
            }
            else
            {
                DoActionBySelectTile(select_tile);
            }
        }
        else
        {
            if (select_tile.HasUnit())
            {
                OnSelectUnit(select_tile.GetUnit());
            }
        }

    }

    public void UnitStateNotify(Unit notify_unit)
    {
        Character character = notify_unit.GetCharacter();
        Debug.Log("unit:" + character.Name + " state :" + character.CurrentState);

        if (character.CurrentState == CharacterState.TurnDone)
        {
            NextUnit();

        }
        else if (character.CurrentState == CharacterState.Death)
        {
            OnUnitDeath();
        }
        else if (character.CurrentState == CharacterState.ReadyForAttack)
        {
            notify_unit.SetAttackableTiles(gridManager.PathFinding.GetAttackableTiles(notify_unit.CurrentTile.Position, character.AttackRange));
            ShowAttackTiles(notify_unit, true);
        }
        else if (character.CurrentState == CharacterState.Move)
        {
            ShowMovableTiles(notify_unit, true);
        }
        else if (character.CurrentState == CharacterState.Generate)
        {
            allUnits.Add(notify_unit);
            if (allUnits.Count >= (playerUnits.Count + enemyUnits.Count)) 
            {
                ChangeState(StageState.TurnStart);
            }
        }
        else if (character.CurrentState == CharacterState.Attack)
        {
            if (notify_unit.IsEqual(selectUnit))
                OnSelectUnit(selectUnit);
        }

    }

    public void ChangeState(StageState state)
    {
        Debug.Log("stage change state:" + state);
        if (stageEnd)
            return;
        switch (state) 
        {
            case StageState.Start:
                OnStart();
                break;
            case StageState.GenerateGrid:
                OnGenerateGrid();
                break;
            case StageState.PlacePlayerUnits:
                OnPlacePlayerUnits();
                break;
            case StageState.PlaceEnemyUnits:
                OnPlaceEnemyUnits();
                break;
            case StageState.TurnStart:
                OnTurnStart();
                break;
            case StageState.Battle:
                OnBattle();
                break;
            case StageState.BattleResult:
                OnBattleResult();
                break;
        }
        curState = state;
    }

    private void OnStart()
    {
        if (gridManager == null)
        {
            GameObject gb = GameObject.FindWithTag("GridManager");
            if (gb != null)
            {
                gridManager = gb.GetComponent<GridManager>();
            }
            else
                Debug.LogError("cant find grid manager object");
        }
        
        uiManager = GameManager.Instance.GetUIManager();

        ResetStage();
        ChangeState(StageState.GenerateGrid);
    }

    private void ResetStage()
    {
        activeUnits.Clear();
        allUnits.Clear();
        turnDoneUnitIDs.Clear();
        curUnit = null;
        turnID = 0;
        stageWin = false;
        stageEnd = false;
    }

    private void OnGenerateGrid()
    {
        gridManager.GenerateGrid();
        ChangeState(StageState.PlacePlayerUnits);
    }

    private void OnPlacePlayerUnits()
    {
        foreach (var unit in playerUnits) 
        {
            PlaceUnitByWorldPosition(unit);
            //allUnits.Add(unit);
        }
        ChangeState(StageState.PlaceEnemyUnits);
    }

    private void OnPlaceEnemyUnits()
    {
        foreach (var unit in enemyUnits)
        {
            PlaceUnitByWorldPosition(unit);
            //allUnits.Add(unit);
        }
        //ChangeState(StageState.TurnStart);
        //Debug.Log("game start");
    }

    private void OnTurnStart()
    {
        //根据先攻排序
        allUnits.Sort((a, b) => b.GetCharacter().Initiative.CompareTo(a.GetCharacter().Initiative));
        activeUnits.Clear();
        foreach (var unit in allUnits)
        {
            if (unit.GetCharacter().CurrentState != CharacterState.Death)
            {
                unit.GetCharacter().ResetTurnRes();
                activeUnits.Enqueue(unit);
            }
                
        }
        turnID++;
        Debug.Log("Turn Start :" + turnID);
        ChangeState(StageState.Battle);
    }

    private void PlaceUnitByWorldPosition(Unit unit)
    {
        Tile tile = gridManager.GetTileWithWorldPosition(unit.gameObject.transform.position);
        if (tile != null)
        {
            tile.PlaceUnit(unit, true);
        }
        else
            Debug.Log("place unit failed unit name:" + unit.gameObject.name);
    }

    private void OnBattle()
    {
        NextUnit();
    }

    private void NextUnit()
    {
        if (activeUnits.Count > 0)
        {
            curUnit = activeUnits.Dequeue();
            curUnit.SetMovableTiles(gridManager.PathFinding.GetMovableTiles(curUnit.CurrentTile.Position, curUnit.GetCharacter().MovePoints));
            if (curUnit.GetCharacter().IsAI)
            {
                curUnit.GetAI().TakeTurn();
            }
        }
        else
            ChangeState(StageState.TurnStart);
    }

    private void OnBattleResult()
    {
        Debug.Log("stage result:" + stageWin);
        stageEnd = true;
    }

    private void CheckWinCondition()
    {
        if (winCondition == StageWinCondition.DefeatAllEnemies)
        {
            bool win = true;
            foreach (var unit in enemyUnits) 
            {
                if (unit.GetCharacter().CurrentState != CharacterState.Death)
                { 
                    win = false; 
                    break;
                }
            }
            if (win) 
            {
                stageWin = true;
                ChangeState(StageState.BattleResult);
                return;
            }
        }

        //check lose all player unit death
        bool lose = true;
        foreach (var unit in playerUnits)
        {
            if (unit.GetCharacter().CurrentState != CharacterState.Death)
            {
                lose = false;
                break;
            }
        }
        if (lose)
        {
            stageWin = false;
            ChangeState(StageState.BattleResult);
        }
    }

    private void OnSelectUnit(Unit unit)
    {
        if (unit == null)
            return;
        if (curUnit.GetCharacter().IsAI)
            return;
        if (selectUnit != null && selectUnit.IsEqual(unit))
        {
            uiManager.ShowCommandPanel(selectUnit.IsEqual(curUnit), unit.GetCharacter());
            return;
        }
        selectUnit = unit;
        //show command panel and update btn show
        uiManager.ShowCommandPanel(selectUnit.IsEqual(curUnit), unit.GetCharacter());

        //show or hide tile highlight
        CharacterState characterState = selectUnit.GetCharacter().CurrentState;
        if (characterState == CharacterState.ReadyForAttack)
        {
            gridManager.ShowAttackableTiles(selectUnit.GetAttackableTiles(), true);
        }
        else if (characterState == CharacterState.Idle || characterState == CharacterState.TurnDone)
        {
            if (selectUnit.GetMovableTiles() == null)
                selectUnit.SetMovableTiles(gridManager.PathFinding.GetMovableTiles(selectUnit.CurrentTile.Position, selectUnit.GetCharacter().MovePoints));
            gridManager.ShowMovableTiles(selectUnit.GetMovableTiles(), true);
        }
    }

    private void OnDeselectUnit()
    {
        if (selectUnit == null)
            return;

        //show command panel and update btn show
        uiManager.ShowCommandPanel(false);

        //show or hide tile highlight
        CharacterState characterState = selectUnit.GetCharacter().CurrentState;
        if (characterState == CharacterState.ReadyForAttack)
        {
            gridManager.ShowAttackableTiles(selectUnit.GetAttackableTiles(), false);
        }
        else if (characterState == CharacterState.Idle || characterState == CharacterState.TurnDone)
        {
            gridManager.ShowMovableTiles(selectUnit.GetMovableTiles(), false);
        }

        selectUnit = null;
    }

    private void ShowAttackTiles(Unit unit, bool is_show)
    {
       gridManager.ShowAttackableTiles(unit.GetAttackableTiles(), is_show);
    }

    private void ShowMovableTiles(Unit unit, bool is_show)
    {
       gridManager.ShowMovableTiles(unit.GetMovableTiles(), is_show);
    }

    private void DoActionBySelectTile(Tile select_tile)
    {
        if (selectUnit == null)
            return;
        if (selectUnit.IsEqual(curUnit) == false)
            return;
        CharacterState cState = selectUnit.GetCharacter().CurrentState;
        if (cState == CharacterState.ReadyForAttack)
        {
            if (selectUnit.IsInAttackableTiles(select_tile))
                selectUnit.Attack(select_tile);
        }
        else if (cState == CharacterState.Idle)
        {
            if (select_tile.IsMovable() && selectUnit.IsMoving == false && selectUnit.IsInMovableTiles(select_tile))
            {
                Tile tileStart = selectUnit.CurrentTile;
                List<Tile> tempTiles = gridManager.PathFinding.FindPath(tileStart.Position, select_tile.Position, selectUnit.GetMovableTiles());
                selectUnit.Move(tempTiles);
            }
        }
    }

    private void OnUnitDeath()
    {
        Queue<Unit> temp = new Queue<Unit>();
        while (activeUnits.Count > 0)
        {
            Unit unit = activeUnits.Dequeue();
            if (unit.GetCharacter().IsDeath() == false)
                temp.Enqueue(unit);
        }
        activeUnits = temp;
        CheckWinCondition();
    }
}
