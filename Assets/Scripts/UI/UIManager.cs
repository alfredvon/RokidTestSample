using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] CommandPanel commandPanel;

    private void Start()
    {
        ShowCommandPanel(false);
    }

    public void ShowCommandPanel(bool is_show, in Character character = null)
    {
        if (is_show == true)
        {
            if (character != null)
            {
                commandPanel.UpdateBtnState(character);
            }
            commandPanel.Show();
        }
            
        else
            commandPanel.Hide();
    }

    public CommandPanel GetCommandPanel()
    { 
        return commandPanel;
    }
}
