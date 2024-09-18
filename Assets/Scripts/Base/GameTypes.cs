using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterState
{
    None,
    Generate,
    Idle,
    Move,
    ReadyForAttack,
    Attack,
    AttackEnd,
    TurnDone,
    Death
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
