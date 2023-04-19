using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private MatchController matchController;

    private PauseMenu pauseMenu;
    

    void Start()
    {
        pauseMenu = GetComponent<PauseMenu>();
        // Places the roundTimer at appropriate place on split screen
        if (PlayerInputManagerController.Singleton.playerInputs.Count > 2)
        {
            roundTimer.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            roundTimer.alignment = TextAlignmentOptions.Top;
        }

        foreach (PlayerStatUI playerStatUI in playerStatPanels)
        {
            playerStatUI.enabled = false;
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
        playerStatPanels[nextPlayerStatIndex].enabled = true;
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

    public void SetMatchManager(MatchController mc)
    {
        matchController = mc;
    }

    public void TogglePause()
    {
        pauseMenu.TogglePause();
    }
}
