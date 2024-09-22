using System.Collections;
using System.Collections.Generic;
using SelfAI.BehaviourTree;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

// test
public class AIController : MonoBehaviour
{

    
    public Unit controlUnit;
    public Vector2Int movePosition;
    public Unit attackTarget;

    BehaviourTree tree;
    List<Unit> potentialTargets;
    
    private void Start()
    {
        potentialTargets = new List<Unit>();
        BuildBehaviourTree();
    }

    public void SetControlUnit(Unit unit)
    {
        controlUnit = unit;
    }

    public void TakeTurn()
    {
        //tree.Process();
        StartCoroutine(ProcessCoroutine());

    }

    IEnumerator ProcessCoroutine()
    {
        while (controlUnit.GetCharacter().CurrentState != CharacterState.TurnDone)
        {
            tree.Process();
            yield return new WaitForFixedUpdate();
        }
    }

    private void BuildBehaviourTree()
    {
        tree = new BehaviourTree("skel");

        Selector actions = new Selector("Agent Logic");

        Sequence findMoveAttack = new Sequence("AttackTarget");
        findMoveAttack.AddChild(new Leaf("FoundTarget?", new ConditionStrategy(HaveTarget)));
        findMoveAttack.AddChild(new Leaf("MoveToTarget", new MoveToPositionStrategy(this)));
        findMoveAttack.AddChild(new Leaf("AttackTarget", new AttackTargetStrategy(this)));
        findMoveAttack.AddChild(new Leaf("TurnEnd", new DoTurnEndStrategy(this)));
        actions.AddChild(findMoveAttack);

        Leaf doNothing = new Leaf("do nothing", new DoTurnEndStrategy(this));
        actions.AddChild(doNothing);
        
        tree.AddChild(actions);
    }

    private bool HaveTarget()
    {
        //����Ǳ��Ŀ�꣬ͨ���ƶ�����+��������򵥲���
        List<Unit> targets = GameManager.Instance.GetAllUnitsWithGroup(UnitGroupType.Player);
        potentialTargets.Clear();
        attackTarget = null;
        movePosition = controlUnit.CurrentTile.Position;
        if (targets.Count <= 0)
            return false;

        Character character = controlUnit.GetCharacter();
        int searchRange = character.MovePoints + character.AttackRange;
        foreach (Unit target in targets)
        {
            if (target.GetCharacter().CurrentState == CharacterState.Death)
                continue;
            if (Vector2Int.Distance(controlUnit.CurrentTile.Position, target.CurrentTile.Position) <= searchRange)
                potentialTargets.Add(target);
        }
        for (int i = 0; i < potentialTargets.Count; i++)
        {
            Debug.Log("before index:" + i + " name: " + potentialTargets[i].GetCharacter().Name);
        }
        //���ݹ�������Ŀ��
        if (potentialTargets.Count > 1)
        {
            potentialTargets.Sort((a, b) => EvaluateTarget(b).CompareTo(EvaluateTarget(a)));
        }

        for (int i = 0; i < potentialTargets.Count; i++)
        {
            Debug.Log("after index:" + i + " name: " + potentialTargets[i].GetCharacter().Name);
        }

        if (potentialTargets.Count <= 0)
            return false;

        //�ж��ܲ��ܹ�����Ǳ��Ŀ��
        for (int i = 0; i < potentialTargets.Count; i++)
        {
            bool hasPos = FindOverlapMoveAndAttackPositions(potentialTargets[i].CurrentTile.Position, out movePosition);
            if (hasPos)
            {
                attackTarget = potentialTargets[i];
                break;
            }
        }

        bool foundTarget = attackTarget != null;
        Debug.Log("found attack target:" + foundTarget);
        return foundTarget;
;
    }

    private float EvaluateTarget(Unit target)
    {
        float distance = 10f / Vector2Int.Distance(controlUnit.CurrentTile.Position, target.CurrentTile.Position);
        float health = 100f / target.GetCharacter().HP.current;
        float score = distance + health;
        Debug.Log("targte name:" + target.GetCharacter().Name + " score:" + score);
        return score;
    }

    private bool FindOverlapMoveAndAttackPositions(Vector2Int attack_point, out Vector2Int best_position)
    {
        best_position = controlUnit.CurrentTile.Position;
        float shortestDistance = float.MaxValue;
        
        HashSet<Tile> movableTiles = new HashSet<Tile>(GameManager.Instance.GetPathFinding().GetMovableTiles(controlUnit.CurrentTile.Position, controlUnit.GetCharacter().MovePoints));
        HashSet<Tile> attackableTiles = new HashSet<Tile>(GameManager.Instance.GetPathFinding().GetAttackableTiles(attack_point, controlUnit.GetCharacter().AttackRange));
        movableTiles.IntersectWith(attackableTiles);

        foreach (Tile tile in movableTiles) 
        {
            float distanceToTarget = Vector2Int.Distance(tile.Position, attack_point);

            // ѡ�����Ŀ������ĸ���
            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                best_position = tile.Position;
            }
        }
        return movableTiles.Count > 0;
    }
}
