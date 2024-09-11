using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    public static readonly int bRun = Animator.StringToHash("Run");
    public static readonly int bAttack = Animator.StringToHash("Attack");

    [SerializeField] bool move;
    [SerializeField] bool attack;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartMoving()
    { 
        move = true;
        animator.SetBool(bRun, move);
    }

    public void StopMoving() 
    { 
        move = false;
        animator.SetBool(bRun, move);
    }

    public void Attack()
    { 
        attack = true;
        animator.SetBool(bAttack, attack);
    }

    public void StopAttack()
    {
        attack = false;
        animator.SetBool(bAttack, attack);
    }

}
