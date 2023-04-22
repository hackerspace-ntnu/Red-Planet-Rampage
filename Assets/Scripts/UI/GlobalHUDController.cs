using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalHUDController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roundTimer;

    [SerializeField]
    private PlayerStatUI[] playerStatPanels;

    [SerializeField]
    private TMP_Text winText;

    [SerializeField]
    private GameObject winScreen;

    private int nextPlayerStatIndex = 0;

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

        StartCoroutine(DisableUnusedStatUIs());
    }

    private IEnumerator DisableUnusedStatUIs()
    {
        // Wait one frame before disabling stat blocks
        yield return null;
        for (int i = nextPlayerStatIndex; i < playerStatPanels.Length; i++)
        {
            playerStatPanels[i].enabled = false;
        }
    }

    public void SetPlayer(PlayerManager playerManager)
    {
        if (nextPlayerStatIndex >= playerStatPanels.Length)
        {
            Debug.LogWarning("Too many player inputs!");
            return;
        }
        playerStatPanels[nextPlayerStatIndex].playerManager = playerManager;
        nextPlayerStatIndex++;
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

    public void DisplayWinScreen(PlayerIdentity winner)
    {
        winText.text = winner.playerName;
        winText.color = winner.color;
        winScreen.SetActive(true);
    }
}
