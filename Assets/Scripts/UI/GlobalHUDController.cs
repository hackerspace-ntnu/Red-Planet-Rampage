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

    [SerializeField] 
    private PauseMenu pauseMenuUI;
    
    private int nextPlayerStatIndex = 0;
    private MatchController matchController;
    private bool gameIsPaused = false;

    
    

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

    public void TogglePause(InputManager input)
    {
        Debug.Log(gameIsPaused);
        gameIsPaused = !gameIsPaused;
        Time.timeScale = gameIsPaused ? 0 : 1;
        pauseMenuUI.gameObject.SetActive(gameIsPaused);
        pauseMenuUI.SetPlayerInput(input);
    }
}
