using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public string Name = "";
    public int MovePoints = 5;
    public int AttackRange = 2;
    public float AttackInterval = 1.5f;
    public CharacterState CurrentState { get; private set; } = CharacterState.Idle;

    public void SetCharacterState(CharacterState state) => CurrentState = state;
}
public enum CharacterState
{ 
    Idle,
    Move,
    ReadyForAttack,
    Done
}
