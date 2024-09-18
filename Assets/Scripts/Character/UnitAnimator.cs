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

    public void StartMoving()
    { 
        
        animator.SetBool(bRun, true);
    }

    public void StopMoving() 
    { 
        
        animator.SetBool(bRun, false);
    }

    public void Attack()
    { 
 
        animator.SetBool(bAttack, true);
    }

    public void StopAttack()
    {
        
        animator.SetBool(bAttack, false);
    }

    public void Hit() 
    {
        animator.SetBool(bHit, true);
    }

    public void StopHit()
    {
        animator.SetBool(bHit, false);
    }

    public void Death()
    {
        animator.SetBool(bDeath, true);
    }
}
