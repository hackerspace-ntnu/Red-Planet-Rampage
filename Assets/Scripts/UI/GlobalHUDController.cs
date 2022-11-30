using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalHUDController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roundTimer;
    [SerializeField]
    private float textOffsetY = 180f;

    void Start()
    {
        if (PlayerInputManagerController.Singleton.playerInputs.Count > 2)
        {
            roundTimer.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            roundTimer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, textOffsetY);
        }
    }

    public void OnTimerUpdate(float time)
    {
        int seconds = Mathf.FloorToInt(time % 60);
        roundTimer.text = Mathf.FloorToInt(time / 60) + ":" + (seconds<10 ? "0" : "") + seconds;
    }
}
