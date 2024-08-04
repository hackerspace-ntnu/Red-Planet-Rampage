using UnityEngine;

public class ArenaCamera : MonoBehaviour
{
    private Animator animator;

    private PlayerIdentity winner;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayScoreboardAnimation()
    {
        animator.SetTrigger("ScoreboardZoom");
    }

    public void PlayVictoryAnimation(PlayerIdentity winner)
    {
        this.winner = winner;
        animator.SetTrigger("VictoryPanning");
    }

    public void ShowWinnerText()
    {
        MatchController.Singleton.GlobalHUD.DisplayWinScreen(winner);
    }

    public void EndVictoryAnimation()
    {
        MatchController.Singleton.WaitAndRestartAfterWinScreen();
    }
}
