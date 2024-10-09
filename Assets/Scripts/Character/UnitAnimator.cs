using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    public static readonly int bRun = Animator.StringToHash("Run");
    public static readonly int bAttack = Animator.StringToHash("Attack");
    public static readonly int bHit = Animator.StringToHash("Hit");
    public static readonly int bDeath = Animator.StringToHash("Death");

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayMoveOrOff(bool on)
    { 
        
        animator.SetBool(bRun, on);
    }

    public void PlayMeleeAttackOrOff(bool on)
    { 
 
        animator.SetBool(bAttack, on);
    }

    public void PlayHitOrOff(bool on) 
    {
        animator.SetBool(bHit, on);
    }

    public void PlayDeathOrOff(bool on)
    {
        animator.SetBool(bDeath, on);
    }
}
