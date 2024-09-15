using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandPanel : PannelBase
{
    public Button BtnAttack;
    public Button BtnTunEnd;
    public Button BtnCancel;

    private void Start()
    {
        BtnAttack.onClick.AddListener(OnBtnAttackClick);
        BtnTunEnd.onClick.AddListener(OnBtnTunEndClick);
        BtnCancel.onClick.AddListener(OnBtnCancelClick);
    }

    public void UpdateBtnState(in Character character)
    {
        BtnAttack.interactable = !character.TurnAttackDone;
        BtnTunEnd.interactable = !character.TurnEnd;
    }

    void OnBtnAttackClick()
    {
        GameManager.Instance.OnClickAttack();
    }

    void OnBtnTunEndClick()
    {
        GameManager.Instance.OnClickTurnEnd();
    }

    void OnBtnCancelClick()
    {
        GameManager.Instance.OnClickCancel();
    }
}
