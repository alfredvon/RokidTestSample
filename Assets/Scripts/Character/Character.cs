using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int ID = 0;
    public string Name = "";
    public int MovePoints = 5;
    public int AttackRange = 2;
    public float AttackInterval = 1.5f;
    //turn resource
    public bool TurnMoveDone = false;
    public bool TurnAttackDone = false;
    public bool TurnEnd = false;
    public CharacterState CurrentState { get; private set; } = CharacterState.None;
    public Unit ParentUnit { get; private set; }

    public void SetParentUnit(Unit parent_unit) => ParentUnit = parent_unit;
    public void SetCharacterState(CharacterState state)
    {
        CurrentState = state;
        GameManager.Instance.UnitStateNotify(this);
    }

    public void ResetTurnRes()
    {
        TurnMoveDone = false;
        TurnAttackDone = false;
        TurnEnd = false;
        CurrentState = CharacterState.Idle;
    }
}

