using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class DiceManager : MonoBehaviour
{
    public enum DiceType
    {
        dice4 = 4, dice6 = 6, dice8 = 8, dice10 = 10, dice20 = 20
    }

    public int RollDice(DiceType dice_type, int dice_count)
    {
        int ret = 0;
        for (int i = 0; i < dice_count; i++)
        {
            ret += Random.Range(1, (int)dice_type);
            Debug.Log("roll dice type : " + dice_type + " count:" + i + " value:" + ret);
        }
        Debug.Log("roll dice type:" + dice_type + " count:" + dice_count + " total value:" + ret);
        return ret;
    }

    


}
