using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��������
public enum AbilityActionType
{ 
    Action,
    BonusAction,
    Reaction
}
//��������
public enum AbilityType
{ 
    MeleeAttack
}
//���÷�Χ
public enum AbilityEffectRangeType
{ 
    Single,
    Area
}
//��������
public enum AbilityEffectType
{
    Damage,
    Heal
}
//��������
public enum AbilityCostType
{ 
    Action,
    BonusAction,
    Reaction
}

public abstract class Ability
{
    public Character owner = null;

    public AbilityType abilityType = AbilityType.MeleeAttack;
    //select range ѡȡ��Χ
    public int rangeMin;
    public int rangeMax;
    public bool rangeExcludeStart = true;

    //effect range ���÷�Χ
    public AbilityEffectRangeType effectRangeType = AbilityEffectRangeType.Single;
    //effect type ��������
    public AbilityEffectType effectType = AbilityEffectType.Damage;
    //�˺�����
    public int diceType; //��������
    public int diceCount; //��������
    public CharacterModifierType modifierType; //��Ҫ�ĵ���ֵ���� 0������Ҫ STR��1 DEX��2 CON��3 INT��4 WIS��5 CHA��6
    //cost ��������
    public AbilityCostType costType = AbilityCostType.Action;
    //animation ʹ�ö���
    public CharacterAnimation abilityAnimation = CharacterAnimation.MeleeAttack;
    public virtual HashSet<Tile> GetTilesInRange(Vector2Int start, TileFinding tile_finding)
    { 
        HashSet<Tile> retTiles = new HashSet<Tile>();
        retTiles = tile_finding.GetTilesInRange(start, rangeMin, rangeMax, rangeExcludeStart);
        return retTiles;
    }

    public virtual void Perform(Tile target_tile)
    {
        //can perform?
        if (CanPerform() == false)
        {
            Debug.Log("No resource to perform ability");
            return;
        }
        //cost
        owner.DoAbilityCost(costType);
        //change character turn state
        owner.SetCharacterTurnState(CharacterTurnState.AbilityPerform);
        //apply
        OnApply(target_tile);
        //if (abilityType == AbilityType.MeleeAttack)
        //    StartCoroutine(ApplyMeleeAbilty(target_tile));
    }

    protected virtual bool CanPerform()
    {
        switch (costType)
        {
            case AbilityCostType.Action:
                return owner.ActionPoints > 0;
            case AbilityCostType.BonusAction:
                return owner.BonusActionPoints > 0;
            case AbilityCostType.Reaction:
                return owner.ReactionPoints > 0;
            default:
                return false;
        }
    }

    protected virtual bool IsTarget(Character target)
    {
        return owner.Group != target.Group;
    }

    protected abstract void OnApply(Tile target_tile);
}
