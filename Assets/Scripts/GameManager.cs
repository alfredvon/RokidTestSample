using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] StageManager stageManager;
    [SerializeField] UIManager uiManager;
   

    

    private void Start()
    {
        stageManager.ChangeState(StageState.Start);
    }

    public UIManager GetUIManager() => uiManager;

   
}
