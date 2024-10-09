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
    STR,    // 力量
    DEX,    // 敏捷
    CON,    // 体质
    INT,    // 智力
    WIS,    // 感知
    CHA     // 魅力
}