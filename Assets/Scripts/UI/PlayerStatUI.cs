using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerNameText;
    [SerializeField]
    private TMP_Text chipsText;

    public void setName(string name)
    {
        playerNameText.SetText(name);
    }

    public void setChips(int chips)
    {
        chipsText.SetText(chips.ToString());
    }
}
