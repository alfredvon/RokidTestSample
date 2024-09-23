using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public bool IsAI = false;
    public int ID = 0;
    public string Name = "";
    public UnitGroupType Group = UnitGroupType.Player;  //��Ӫ
    public int Initiative = 0;  //�ȹ�ֵ
    public int MovePoints = 5;  //�ƶ���
    public int AttackRange = 2; //��ͨ������Χ
    public float AttackInterval = 1.5f; //�����������
    public Int2Val HP = new Int2Val(100, 100);
    public int Damage = 50;
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
        StageManager.Instance.UnitStateNotify(this.ParentUnit);
    }

    public void ResetTurnRes()
    {
        TurnMoveDone = false;
        TurnAttackDone = false;
        TurnEnd = false;
        if (CurrentState != CharacterState.Death)
            CurrentState = CharacterState.Idle;
    }

    public void DoTurnEnd()
    {
        TurnEnd = true;
        TurnAttackDone = true;
        TurnMoveDone = true;
        SetCharacterState(CharacterState.TurnDone);
    }

    public void TakeDamage(int damage)
    {
        HP.current -= damage;
        if (HP.current <= 0)
        {
            SetCharacterState(CharacterState.Death);
        }
    }

    public bool IsDeath()
    { 
        return HP.current <= 0;
    }
}

