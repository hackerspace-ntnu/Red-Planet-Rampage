using System.Collections;
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

    [SerializeField]
    private TMP_Text startText;

    void Start()
    {
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

    public IEnumerator DisplayStartScreen(float seconds)
    {
        var colorTransparent = startText.color;
        var color = colorTransparent;
        color.a = 1f;
        LeanTween.value(startText.gameObject, TMPFade, colorTransparent, color, 1.5f);
        startText.gameObject.LeanScale(new Vector3(1.1f, 1.1f, 1.1f), 1f).setEaseOutBounce();
        yield return new WaitForSeconds(seconds);
        LeanTween.value(startText.gameObject, TMPFade, color, colorTransparent, 1.5f);
    }

    private void TMPFade(Color color)
    {
        startText.color = color;   
    }

    public void DisplayWinScreen(PlayerIdentity winner)
    {
        winText.text = winner.playerName;
        winText.color = winner.color;
        winScreen.SetActive(true);
    }
}
