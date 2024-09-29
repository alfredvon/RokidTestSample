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

    [Header("����Ԥ����")]
    [SerializeField] GameObject dice4_Prefab;
    [SerializeField] GameObject dice6_Prefab;
    [SerializeField] GameObject dice8_Prefab;
    [SerializeField] GameObject dice10_Prefab;
    [SerializeField] GameObject dice12_Prefab;
    [SerializeField] GameObject dice20_Prefab;
    [Header("���ӳ�ʼλ��")]
    [SerializeField] List<Transform> initPosList;
    [Header("��Ծ����")]
    [SerializeField] float forceAmount = 300f;
    [Header("����Ԥ�Ƴ���ʱ��")]
    [SerializeField] float duration = 3f;

    /// <summary>
    /// �������Ӳ�Ͷ����ͨ���ص���ȡ���������
    /// </summary>
    /// <param name="diceType">��������</param>
    /// <param name="diceCount">���Ӹ���</param>
    /// <param name="callBack">��ȡ�������</param>
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
        Debug.Log("Ͷ������" + dicePoint);
    }
    #endregion


}
