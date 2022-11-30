using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalHUDController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roundTimer;
    [SerializeField]
    private float textStartY = -25f;
    [SerializeField]
    private float textOffsetY = 180f;

    void Start()
    {
        if (PlayerInputManagerController.Singleton.playerInputs.Count > 2)
        {
            roundTimer.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
        }
        else
        {
            roundTimer.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Top;
        }
    }

    public void OnTimerUpdate(float time)
    {
        int seconds = Mathf.FloorToInt(time % 60);
        roundTimer.text = Mathf.FloorToInt(time / 60) + ":" + (seconds<10 ? "0" : "") + seconds;
    }
}
