using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAbility : Ability
{
    protected override void OnApply(Tile target_tile)
    {
        owner.StartCoroutine(ApplyMeleeAbiltyCoroutine(target_tile));
    }

    IEnumerator ApplyMeleeAbiltyCoroutine(Tile target_tile)
    {
        //face direction
        owner.ParentUnit.RotateUnit(target_tile.WorldPosition);
        //play anim
        owner.ParentUnit.PlayCharacterAnimation(abilityAnimation, true);
        //wait apply effect time
        yield return new WaitForSeconds(.5f);
        if (effectRangeType == AbilityEffectRangeType.Single)
        {
            if (target_tile.HasUnit())
            {
                Character target_c = target_tile.GetUnit().Character;
                if (effectType == AbilityEffectType.Damage && IsTarget(target_c))
                {
                    //roll ����
                    int hitRate = GameManager.Instance.DiceManager.RollDice(DiceManager.DiceType.dice20, 1);
                    Debug.Log("roll ���У�" + hitRate + " �Է�AC:" + target_c.AC);
                    if (hitRate >= target_c.AC)
                    {
                        //roll damage
                        target_tile.GetUnit().TakeDamage(owner.CalculateAbilityDamage());
                    }
                }
                
            }
        }
        yield return new WaitForSeconds(1.0f);
        //stop anim
        owner.ParentUnit.PlayCharacterAnimation(abilityAnimation, false);
        //perform done
        owner.SetCharacterTurnState(CharacterTurnState.AbilityPerformDone);


    }
}
