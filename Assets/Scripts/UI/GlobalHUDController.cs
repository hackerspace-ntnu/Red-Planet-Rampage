using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalHUDController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roundTimer;

    [SerializeField]
    private GameObject PlayerStatUI;

    void Start()
    {
        // Places the roundTimer at appropriate place on split screen
        if (PlayerInputManagerController.Singleton.playerInputs.Count > 2)
        {
            roundTimer.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            roundTimer.alignment = TextAlignmentOptions.Top;
        }
    }

    public void SetPlayer(PlayerManager playerManager)
    {
        GameObject playerStatUIObject = Instantiate(PlayerStatUI, gameObject.transform);
        PlayerStatUI playerStatUI = playerStatUIObject.GetComponent<PlayerStatUI>();
        playerStatUI.setName(playerManager.identity.playerName);
        playerStatUI.SetChips(playerManager.identity.chips);
        playerManager.identity.onChipChange += playerStatUI.SetChips;
    }

    public void OnTimerUpdate(float time)
    {
        if (time < 0)
        {
            roundTimer.gameObject.SetActive(false);
        }
        else
        {
            int seconds = Mathf.FloorToInt(time % 60);
            roundTimer.text = Mathf.FloorToInt(time / 60) + ":" + (seconds < 10 ? "0" : "") + seconds;
        }
    }
}
