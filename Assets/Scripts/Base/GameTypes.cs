using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterTurnState
{
    None,
    Generate,
    Idle,
    Move,
    AbilityTargetSelect,
    AbilityPerform,
    AbilityPerformDone,
    TurnDone
}

[Serializable]
public struct Int2Val
{
    public int current;
    public int max;
    public Int2Val(int max, int current)
    {
        this.max = max;
        this.current = current;
    }
}

public enum UnitGroupType
{ 
    Player,
    Enemy
}

public enum StageState
{ 
    None,
    Start,
    GenerateGrid,
    PlacePlayerUnits,
    PlaceEnemyUnits,
    TurnStart,
    Battle,
    BattleResult
}

public enum StageWinCondition
{
    DefeatAllEnemies,
    TurnLimit
}


public enum TileHighlightType
{ 
    Move,
    Ability
}

public enum CharacterAnimation
{
    MeleeAttack
}

public enum CharacterModifierType
{
    None,
    STR,    // ����
    DEX,    // ����
    CON,    // ����
    INT,    // ����
    WIS,    // ��֪
    CHA     // ����
}