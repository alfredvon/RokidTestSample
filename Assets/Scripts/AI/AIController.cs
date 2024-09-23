using System.Collections;
using System.Collections.Generic;
using SelfAI.BehaviourTree;
using UnityEngine;

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
        if (controlUnit.GetCharacter().CurrentState == CharacterState.Death)
            return;
        Debug.Log("AI TAKE TURN");
        StopAllCoroutines();
        StartCoroutine(ProcessCoroutine());
    }

    public void StopAI()
    {
        StopAllCoroutines();
    }

    IEnumerator ProcessCoroutine()
    {
        while (controlUnit.GetCharacter().CurrentState != CharacterState.TurnDone && controlUnit.GetCharacter().CurrentState != CharacterState.Death)
        {
            tree.Process();
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("AI STOP Coroutine");
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
        //查找潜在目标，通过移动距离+攻击距离简单查找
        List<Unit> targets = StageManager.Instance.GetUnitsWithGroupType(UnitGroupType.Player);
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
        //for (int i = 0; i < potentialTargets.Count; i++)
        //{
        //    Debug.Log("before index:" + i + " name: " + potentialTargets[i].GetCharacter().Name);
        //}
        //根据规则排序目标
        if (potentialTargets.Count > 1)
        {
            potentialTargets.Sort((a, b) => EvaluateTarget(b).CompareTo(EvaluateTarget(a)));
        }

        //for (int i = 0; i < potentialTargets.Count; i++)
        //{
        //    Debug.Log("after index:" + i + " name: " + potentialTargets[i].GetCharacter().Name);
        //}

        if (potentialTargets.Count <= 0)
            return false;

        //判断能不能攻击到潜在目标
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
        //Debug.Log("targte name:" + target.GetCharacter().Name + " score:" + score);
        return score;
    }

    private bool FindOverlapMoveAndAttackPositions(Vector2Int attack_point, out Vector2Int best_position)
    {
        best_position = controlUnit.CurrentTile.Position;
        float shortestDistance = float.MaxValue;
        
        HashSet<Tile> movableTiles = new HashSet<Tile>(controlUnit.GetMovableTiles());
        HashSet<Tile> attackableTiles = new HashSet<Tile>(StageManager.Instance.GetGridManager().PathFinding.GetAttackableTiles(attack_point, controlUnit.GetCharacter().AttackRange));
        movableTiles.IntersectWith(attackableTiles);

        foreach (Tile tile in movableTiles) 
        {
            float distanceToTarget = Vector2Int.Distance(tile.Position, attack_point);

            // 选择距离目标最近的格子
            if (distanceToTarget < shortestDistance)
            {
                shortestDistance = distanceToTarget;
                best_position = tile.Position;
            }
        }
        return movableTiles.Count > 0;
    }
}
