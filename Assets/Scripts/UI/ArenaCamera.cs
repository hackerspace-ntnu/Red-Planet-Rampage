using System.Collections;
using UnityEngine;

public class ArenaCamera : MonoBehaviour
{
    private Animator animator;
    private PlayerIdentity winner;
    public static Camera CurrentCamera;
    [SerializeField]
    private Material outlineMaterial;
    [SerializeField]
    private float defaultWidth = 4f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        CurrentCamera = GetComponent<Camera>();
        SetArenaCameraOutline(defaultWidth);
        outlineMaterial.SetColor("_OutlineColor", Color.white);
    }

    public void PlayScoreboardAnimation()
    {
        outlineMaterial.SetColor("_OutlineColor", Color.black);
        animator.SetTrigger("ScoreboardZoom");
    }

    public void PlayVictoryAnimation(PlayerIdentity winner)
    {
        this.winner = winner;
        SetArenaCameraOutline(0f);
        outlineMaterial.SetColor("_OutlineColor", Color.white);
        animator.SetTrigger("VictoryPanning");
    }

    public void ShowWinnerText()
    {
        MatchController.Singleton.GlobalHUD.DisplayWinScreen(winner);
        LeanTween.value(gameObject, SetArenaCameraOutline, 0f, 20f, 2f).setEaseInOutExpo();
    }

    public void EndVictoryAnimation()
    {
        MatchController.Singleton.WaitAndRestartAfterWinScreen();
    }

    private void SetArenaCameraOutline(float value)
    {
        outlineMaterial.SetFloat("_OutlineWidth", value);
    }
}
