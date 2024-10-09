using System.Collections;
using System.Collections.Generic;
using SelfAI.BehaviourTree;
using UnityEngine;

public class AIController : MonoBehaviour
{

    [SerializeField] string treeName = "normal";
    [HideInInspector] public Unit controlUnit;
    [HideInInspector] public Vector2Int movePosition;
    [HideInInspector] public Unit attackTarget;
    [HideInInspector] public List<Unit> potentialTargets;

    BehaviourTree tree;
    
    
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
        if (controlUnit.Character.IsDeath())
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
        while (controlUnit.Character.CurrentState != CharacterTurnState.TurnDone && controlUnit.Character.IsDeath() == false)
        {
            tree.Process();
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("AI STOP Coroutine");
    }

    private void BuildBehaviourTree()
    {
        tree = BehaviourTreeBank.GetBehaviourTree(treeName, this);
    }

    
}
