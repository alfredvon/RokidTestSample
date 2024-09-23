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
        StageManager.Instance.OnClickCommandPanelAttack();
    }

    void OnBtnTunEndClick()
    {
        StageManager.Instance.OnClickCommandPanelTurnEnd();
    }

    void OnBtnCancelClick()
    {
        StageManager.Instance.OnClickCommandPanelCancel();
    }
}
