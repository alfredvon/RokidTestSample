using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SelfAI.BehaviourTree
{
    public class BehaviourTreeBank
    {
        public static BehaviourTree GetBehaviourTree(string tree_name, AIController ai)
        {
            if (tree_name.Equals("normal"))
            {
                return BuildNormal(ai);
            }
            return null;
        }

        private static BehaviourTree BuildNormal(AIController ai)
        {
            BehaviourTree tree = new BehaviourTree("normal");

            Selector actions = new Selector("Agent Logic");

            Sequence findMoveAttack = new Sequence("AttackTarget");
            findMoveAttack.AddChild(new Leaf("FoundTarget?", new ConditionStrategy(()=>HasTarget(ai))));
            findMoveAttack.AddChild(new Leaf("MoveToTarget", new MoveToPositionStrategy(ai)));
            findMoveAttack.AddChild(new Leaf("AttackTarget", new AttackTargetStrategy(ai)));
            findMoveAttack.AddChild(new Leaf("TurnEnd", new DoTurnEndStrategy(ai)));
            actions.AddChild(findMoveAttack);

            Leaf doNothing = new Leaf("do nothing", new DoTurnEndStrategy(ai));
            actions.AddChild(doNothing);

            tree.AddChild(actions);
            return tree;
        }

        private static bool HasTarget(AIController ai)
        {
            //查找潜在目标，通过移动距离+攻击距离简单查找
            List<Unit> targets = StageManager.Instance.GetUnitsWithGroupType(UnitGroupType.Player);
            ai.potentialTargets.Clear();
            ai.attackTarget = null;
            ai.movePosition = ai.controlUnit.CurrentTile.Position;
            if (targets.Count <= 0)
                return false;

            Character character = ai.controlUnit.GetCharacter();
            int searchRange = character.MovePoints + character.AttackRange;
            foreach (Unit target in targets)
            {
                if (target.GetCharacter().CurrentState == CharacterState.Death)
                    continue;
                if (Vector2Int.Distance(ai.controlUnit.CurrentTile.Position, target.CurrentTile.Position) <= searchRange)
                    ai.potentialTargets.Add(target);
            }
            //for (int i = 0; i < potentialTargets.Count; i++)
            //{
            //    Debug.Log("before index:" + i + " name: " + potentialTargets[i].GetCharacter().Name);
            //}
            //根据规则排序目标
            if (ai.potentialTargets.Count > 1)
            {
                ai.potentialTargets.Sort((a, b) => EvaluateTarget(a,b).CompareTo(EvaluateTarget(b,a)));
            }

            //for (int i = 0; i < potentialTargets.Count; i++)
            //{
            //    Debug.Log("after index:" + i + " name: " + potentialTargets[i].GetCharacter().Name);
            //}

            if (ai.potentialTargets.Count <= 0)
                return false;

            //判断能不能攻击到潜在目标
            for (int i = 0; i < ai.potentialTargets.Count; i++)
            {
                bool hasPos = FindOverlapMoveAndAttackPositions(ai.controlUnit, ai.potentialTargets[i].CurrentTile.Position, out ai.movePosition);
                if (hasPos)
                {
                    ai.attackTarget = ai.potentialTargets[i];
                    break;
                }
            }

            bool foundTarget = ai.attackTarget != null;
            Debug.Log("found attack target:" + foundTarget);
            return foundTarget;
        }

        private static float EvaluateTarget(Unit self, Unit target)
        {
            float distance = 10f / Vector2Int.Distance(self.CurrentTile.Position, target.CurrentTile.Position);
            float health = 100f / target.GetCharacter().HP.current;
            float score = distance + health;
            //Debug.Log("targte name:" + target.GetCharacter().Name + " score:" + score);
            return score;
        }

        private static bool FindOverlapMoveAndAttackPositions(Unit self, Vector2Int attack_point, out Vector2Int best_position)
        {
            best_position = self.CurrentTile.Position;
            float shortestDistance = float.MaxValue;

            HashSet<Tile> movableTiles = new HashSet<Tile>(self.GetMovableTiles());
            HashSet<Tile> attackableTiles = new HashSet<Tile>(StageManager.Instance.GetGridManager().PathFinding.GetAttackableTiles(attack_point, self.GetCharacter().AttackRange));
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
}

