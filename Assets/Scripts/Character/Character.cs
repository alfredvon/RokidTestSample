using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using static DiceManager;

public class Character : MonoBehaviour
{
    // Attribute
    public bool IsAI = false;
    public int ID = 0;
    public string Name = "";
    public UnitGroupType Group = UnitGroupType.Player;  //��Ӫ
    public int Initiative = 0;  //�ȹ�ֵ
    public int MovePoints = 5;  //�ƶ���
    public int AC = 10; //����ֵ
    
    public Int2Val HP = new Int2Val(100, 100);
    public int ProficiencyBonus;    //������ֵ
    public CharacterBaseAttribute BaseAttribute;
    public CharacterBaseAttribute Modifier;
    public bool IsDeath() => HP.current <= 0;
    // turn resource
    public bool TurnMoveDone = false;
    public bool TurnAttackDone => ActionPoints < 1;
    public bool TurnEnd = false;
    public int ActionPoints = 1;         //��������
    public int BonusActionPoints = 1;    //��������
    public int ReactionPoints = 1;       //��Ӧ����
    public int RemainingMovePoints;     //��ǰ�غ�ʣ���ƶ���
    // ability
    public List<Ability> ActionList = new List<Ability>();
    public List<Ability> BonusActionList = new List<Ability>();
    public List<Ability> ReactionList = new List<Ability>();
    public Ability CurrentAbility = null;   //��ǰѡ��ļ���
    public CharacterTurnState CurrentState { get; private set; } = CharacterTurnState.None;
    public Unit ParentUnit { get; private set; }

    public void SetParentUnit(Unit parent_unit) => ParentUnit = parent_unit;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        //test
        MeleeAbility ability = new MeleeAbility();
        ability.owner = this;
        ability.rangeMin = 1;
        ability.rangeMax = 2;
        ability.diceCount = 2;
        ability.diceType = 6;
        ActionList.Add(ability);
        CurrentAbility = ActionList[0];
        Debug.Log("ability set");

        InitModifier();

        ResetTurnRes();
    }

    private void InitModifier()
    {
        Modifier = new CharacterBaseAttribute();
        Modifier.STR = ModifierCalculate(BaseAttribute.STR);
        Modifier.DEX = ModifierCalculate(BaseAttribute.DEX);
        Modifier.CON = ModifierCalculate(BaseAttribute.CON);
        Modifier.INT = ModifierCalculate(BaseAttribute.INT);
        Modifier.WIS = ModifierCalculate(BaseAttribute.WIS);
        Modifier.CHA = ModifierCalculate(BaseAttribute.CHA);

        ProficiencyBonus = 2;
    }

    private int ModifierCalculate(int val)
    {
        int sign = val >= 10 ? 1 : -1;
        return Mathf.FloorToInt(Mathf.Abs(val - 10) * 0.5f) * sign;
    }

    public void SetCharacterTurnState(CharacterTurnState state)
    {
        CurrentState = state;
        StageManager.Instance.UnitStateNotify(this.ParentUnit);
    }

    public HashSet<Tile> GetAbilityRangeTiles(TileFinding tile_finding)
    {
        HashSet<Tile> result = new HashSet<Tile>();
        if (CurrentAbility == null)
        {
            Debug.Log("ShowAbilityRange : no ability select");
            return result;
        }
        SetCharacterTurnState(CharacterTurnState.AbilityTargetSelect);
        result = CurrentAbility.GetTilesInRange(ParentUnit.CurrentTile.Position, tile_finding);
        return result;
    }

    public void ResetTurnRes()
    {
        TurnMoveDone = false;
        TurnEnd = false;
        if (IsDeath() == false)
            CurrentState = CharacterTurnState.Idle;
        ActionPoints = 1;
        BonusActionPoints = 1;
        ReactionPoints = 1;
        RemainingMovePoints = MovePoints;
    }

    public void DoTurnEnd()
    {
        TurnEnd = true;
        TurnMoveDone = true;
        SetCharacterTurnState(CharacterTurnState.TurnDone);
    }

    public void TakeDamage(int damage)
    {
        if (IsDeath())
            return;
        HP.current -= damage;
        if (HP.current <= 0)
        {
            OnDeath();
        }
    }

    public Ability SelectAbility(AbilityActionType type, int id)
    {
        Ability ability = null;
        List<Ability> refList = ActionList;
        switch (type)
        {
            case AbilityActionType.BonusAction:
                refList = BonusActionList;
                break;
            case AbilityActionType.Reaction:
                refList = ReactionList;
                break;
            default:
                break;
        }
        if (id < 0 || id >= refList.Count)
        {
            Debug.Log("no id:" + " in ability list " + type);
            return ability;
        }
        ability = refList[id];
        return ability;
    }

    public void DoAbilityCost(AbilityCostType cost_type)
    {
        switch (cost_type)
        {
            case AbilityCostType.Action:
                ActionPoints--;
                break;
            case AbilityCostType.BonusAction:
                BonusActionPoints--;
                break;
            case AbilityCostType.Reaction:
                ReactionPoints--;
                break;
            default:
                break;
        }
    }

    public int CalculateAbilityDamage()
    {
        int ret = 0;
        if (CurrentAbility == null)
            return ret;
        //�����˺�
        int skillDam = GameManager.Instance.DiceManager.RollDice((DiceType)CurrentAbility.diceType, CurrentAbility.diceCount);
        Debug.Log("�����˺�:"+ skillDam);
        ret += skillDam;
        //����ֵ
        int mod = GetModifierByType(CurrentAbility.modifierType);
        Debug.Log("����ֵ ����:" + CurrentAbility.modifierType + " ��ֵ��" + skillDam);
        ret += mod;
        //�Ƿ�Ҫ��������ֵ
        // to do
        Debug.Log("�������˺���" + ret);
        return ret;
    }

    public int GetModifierByType(CharacterModifierType type)
    { 
        switch(type) 
        {
            case CharacterModifierType.STR:
                return Modifier.STR;
            case CharacterModifierType.DEX:
                return Modifier.DEX;
            case CharacterModifierType.CON:
                return Modifier.CON;
            case CharacterModifierType.INT:
                return Modifier.INT;
            case CharacterModifierType.WIS:
                return Modifier.WIS;
            case CharacterModifierType.CHA:
                return Modifier.CHA;
            default:
                return 0;
        }
    }

    private void OnDeath()
    {
        StageManager.Instance.UnitStateNotify(this.ParentUnit);
    }
}

