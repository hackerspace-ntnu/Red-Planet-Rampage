using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

public class Scoreboard : MonoBehaviour
{
    private ScoreboardManager scoreboardManager;

    private PlayerManager player;

    public int TotalCrimes { get => rewardData.Count; }

    [Header("Chip gain")]

    [SerializeField]
    private TMP_Text subtitle;

    [SerializeField]
    private GameObject wantedPoster;

    [SerializeField]
    private Transform crimeContent;

    [SerializeField]
    private GameObject crimeTextPrefab;

    [Header("Match progress")]

    [SerializeField]
    private TMP_Text progressHeader;

    [SerializeField]
    private TMP_Text playerName;

    [SerializeField]
    private GameObject progressPoster;

    [SerializeField]
    private GameObject winProgressSection;

    [SerializeField]
    private GameObject[] progressCrosses;

    [SerializeField]
    private GameObject scoreProgressSection;

    [SerializeField]
    private TMP_Text scoreText;

    [SerializeField]
    private TMP_Text roundText;

    private int currentStep;

    private List<(string label, string amount)> rewardData = new();
    private List<(TMP_Text label, TMP_Text amount)> rewardComponents = new();

    void Start()
    {
        scoreboardManager = ScoreboardManager.Singleton;
    }

    public void SetupPoster(PlayerManager player, string subtitle)
    {
        wantedPoster.GetComponent<Image>().color = player.identity.color;
        this.subtitle.text = subtitle;
        this.player = player;
        progressPoster.GetComponent<Image>().color = player.identity.color;
        playerName.text = player.identity.playerName;

        scoreboardManager.ShowNextCrime += NextStep;
        scoreboardManager.ShowVictoryProgress += ShowVictoryProgress;
    }

    public void AddReward(string label, int value)
    {
        AddReward(label, value.ToString());
    }

    public void AddReward(string label, string value)
    {
        var reward = Instantiate(crimeTextPrefab, crimeContent);
        var texts = reward.GetComponentsInChildren<TMP_Text>();

        rewardComponents.Add((texts[0], texts[1]));
        rewardData.Add((label, $"{value}<sprite name=\"chip\">"));
    }

    public void AddBlankReward()
    {
        var reward = Instantiate(crimeTextPrefab, crimeContent);
        var texts = reward.GetComponentsInChildren<TMP_Text>();

        rewardComponents.Add((texts[0], texts[1]));
        rewardData.Add(("", ""));
    }

    private void NextStep()
    {
        DisplayCrime(currentStep);
        currentStep++;
    }

    private void DisplayCrime(int index)
    {
        if (index <= rewardData.Count - 1)
        {
            var (label, amount) = rewardComponents[index];
            label.text = rewardData[index].label;
            amount.text = rewardData[index].amount;
        }
    }

    private void ShowVictoryProgress()
    {
        if (MatchRules.Current.MatchWinCondition.StopCondition == MatchStopConditionType.FirstToXWins)
        {
            StartCoroutine(AnimateVictoryProgress());
        }
        else
        {
            StartCoroutine(AnimateScoreProgress());
        }
    }

    private IEnumerator AnimateVictoryProgress()
    {
        wantedPoster.SetActive(false);
        progressPoster.SetActive(true);
        winProgressSection.SetActive(true);
        scoreProgressSection.SetActive(false);
        progressHeader.text = "Victories";

        const float delayTime = 0.5f;
        for (int i = 0; i < MatchController.Singleton.WinsForPlayer(player); i++)
        {
            progressCrosses[i].SetActive(true);
            progressCrosses[i].LeanScale(new Vector3(2f, 2f, 2f), 0.5f).setEasePunch();
            yield return new WaitForSeconds(delayTime);
        }
        yield return new WaitForSeconds(scoreboardManager.matchProgressDelay - delayTime * 3);
    }

    private IEnumerator AnimateScoreProgress()
    {
        wantedPoster.SetActive(false);
        progressPoster.SetActive(true);
        winProgressSection.SetActive(false);
        scoreProgressSection.SetActive(true);
        progressHeader.text = MatchRules.Current.MatchWinCondition.WinCondition.ToString();

        if (MatchRules.Current.MatchWinCondition.StopCondition == MatchStopConditionType.AfterXRounds)
        {
            roundText.gameObject.SetActive(true);
            roundText.text = $"Round {MatchController.Singleton.RoundCount}/{MatchRules.Current.MatchWinCondition.AmountForStopCondition}";
        }

        int score = MatchRules.Current.MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins => MatchController.Singleton.WinsForPlayer(player),
            MatchWinConditionType.Kills => MatchController.Singleton.KillsForPlayer(player),
            MatchWinConditionType.Score => player.identity.Score,
            _ => 0,
        };

        int scoreGain = MatchRules.Current.MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins => MatchController.Singleton.LastRound.IsWinner(player.id) ? 1 : 0,
            MatchWinConditionType.Kills => MatchController.Singleton.LastRound.KillCount(player),
            // TODO find gain when score is implemented
            MatchWinConditionType.Score => player.identity.Score,
            _ => 0,
        };

        int previousScore = score - scoreGain;

        // TODO change this once we have more than 3 kills possible per round!
        const float delayTime = 0.3f;
        scoreText.text = $"{previousScore}";
        for (int i = 1; i <= scoreGain; i++)
        {
            yield return new WaitForSeconds(delayTime);
            scoreText.text = (previousScore + i).ToString();
            scoreText.gameObject.LeanScale(1.5f * Vector3.one, delayTime).setEasePunch();
        }
        // TODO avoid hardcoding progress display time
        yield return new WaitForSeconds(scoreboardManager.matchProgressDelay - 1.5f);
    }
}
