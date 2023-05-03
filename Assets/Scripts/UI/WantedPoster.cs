using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WantedPoster : MonoBehaviour
{
    [Header("Variables")]

    private Player player;
    private MatchController matchController;
    private int roundSpoils;

    private int totalSteps;
    public int TotalSteps { get => totalSteps; }

    [Header("Prefabs")]
    [SerializeField]
    private GameObject crimeTextPrefab;

    [Header("References")]
    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    public Image photo;

    [SerializeField]
    private Transform crimeContent;

    [SerializeField]
    private AnimationClip cameraEndRoundPan;


    private int currentStep;
    public int CurrentStep { get => currentStep; }

    private List<(string, string)> crimeData = new List<(string, string)>();
    private List<(TMP_Text, TMP_Text)> crimeTextComponents = new List<(TMP_Text, TMP_Text)>();

    private TownScoreboard townScoreboard;

    public void SetupPoster(Player player, MatchController matchController, string subtitle)
    {
        this.player = player;
        this.matchController = matchController;
        this.subtitle.text = subtitle;

        GetComponent<Image>().color = player.playerIdentity.color;
        townScoreboard = GetComponentInParent<TownScoreboard>();
    }

    private void AddPosterCrime(string crimeLabel, int Value)
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        crimeTextComponents.Add((texts[0], texts[1]));
        crimeData.Add((crimeLabel, Value.ToString()));
    }

    public void UpdatePosterValues()
    {
        if (matchController == null)
            matchController = MatchController.Singleton;

        Round lastRound = matchController.GetLastRound();

        // Add the different crimes players can gain money from

        // Previous savings
        AddPosterCrime("Savings", player.playerIdentity.chips);

        // Participation award
        AddPosterCrime("Base", matchController.ChipBaseReward);

        // Kill Award
        if (lastRound.KillCount(player.playerManager) != 0)
        {
            AddPosterCrime("Kills", matchController.ChipKillReward * lastRound.KillCount(player.playerManager));
        }

        // Round winner award
        if (lastRound.IsWinner(player.playerIdentity)) {
            AddPosterCrime("Victor", matchController.ChipWinReward);
        }

        // New total
        roundSpoils = matchController.ChipBaseReward
            + matchController.ChipKillReward * lastRound.KillCount(player.playerManager)
            + (lastRound.IsWinner(player.playerIdentity) ? matchController.ChipWinReward : 0);

        AddPosterCrime("Total", roundSpoils);

        // Animate the crimes
        townScoreboard.ShowNextCrime += NextStep;
    }

    private void NextStep()
    {
        DisplayCrime(currentStep);
        currentStep++;

        if (currentStep > totalSteps)
        {
            townScoreboard.TryToStartBidding();
        }
    }

    private void DisplayCrime(int index)
    {
        if (index <= crimeData.Count - 1)
        {
            crimeTextComponents[index].Item1.text = crimeData[index].Item1;
            crimeTextComponents[index].Item2.text = crimeData[index].Item2;
        }
    }
}

