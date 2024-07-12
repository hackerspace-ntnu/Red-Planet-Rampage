using TMPro;
using UnityEngine;

public class GlobalHUDController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text roundTimer;
    public TMP_Text RoundTimer => roundTimer;

    [SerializeField]
    private TMP_Text winText;

    [SerializeField]
    private GameObject winScreen;

    private void Start()
    {
        // Imprison mouse muhahahaha
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Places the roundTimer at appropriate place on split screen
        if (PlayerInputManagerController.Singleton.LocalPlayerInputs.Count > 2)
        {
            roundTimer.alignment = TextAlignmentOptions.Center;
        }
        else
        {
            roundTimer.alignment = TextAlignmentOptions.Top;
        }

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
