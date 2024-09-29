using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DiceManager : MonoBehaviour
{
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ThrowDice(DiceType.dice20, 1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ThrowDice(DiceType.dice20, 2);
        }
    }

    #region ThrowDice
    public enum DiceType
    {
        dice4, dice6, dice8, dice10, dice20
    }

    [Header("骰子预制体")]
    [SerializeField] GameObject dice4_Prefab;
    [SerializeField] GameObject dice6_Prefab;
    [SerializeField] GameObject dice8_Prefab;
    [SerializeField] GameObject dice10_Prefab;
    [SerializeField] GameObject dice12_Prefab;
    [SerializeField] GameObject dice20_Prefab;
    [Header("骰子初始位置")]
    [SerializeField] List<Transform> initPosList;
    [Header("跳跃力度")]
    [SerializeField] float forceAmount = 300f;
    [Header("动画预计持续时间")]
    [SerializeField] float duration = 3f;

    /// <summary>
    /// 生成骰子并投掷，通过回调获取点数结果。
    /// </summary>
    /// <param name="diceType">骰子类型</param>
    /// <param name="diceCount">骰子个数</param>
    /// <param name="callBack">获取点数结果</param>
    public void ThrowDice(DiceType diceType,int diceCount, UnityEvent<int> callBack = null)
    {
        GameObject prefab = GetDicePrefab(diceType);
        List<DiceStats> diceList = new List<DiceStats>();
        for(int i = 0; i < diceCount; i++)
        {
            GameObject dice = Instantiate(prefab, initPosList[i].position, Quaternion.identity, transform);            
            Rigidbody rb = dice.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * forceAmount);
            rb.AddTorque(new Vector3(Random.value * forceAmount, Random.value * forceAmount, Random.value * forceAmount));
            diceList.Add(dice.GetComponent<DiceStats>());
        }
        StartCoroutine(GetDicePoint(diceList,callBack));
    }

    GameObject GetDicePrefab(DiceType diceType)
    {
        switch(diceType)
        {
            case DiceType.dice4: return dice4_Prefab;
            case DiceType.dice6: return dice6_Prefab;
            case DiceType.dice8: return dice8_Prefab;
            case DiceType.dice10: return dice10_Prefab;
            case DiceType.dice20: return dice20_Prefab;
            default: return null;
        }
    }

    IEnumerator GetDicePoint(List<DiceStats> diceStats,UnityEvent<int> callBack)
    {
        yield return new WaitForSeconds(duration);
        int dicePoint = 0;
        for(int i = 0; i < diceStats.Count; i++)
        {
            dicePoint += diceStats[i].side;
            Destroy(diceStats[i].gameObject, 3f);
        }        
        callBack?.Invoke(dicePoint);
        Debug.Log("投掷出了" + dicePoint);
    }
    #endregion


}
