using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

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
    private TMP_Text hintText;

    [SerializeField]
    private TMP_Text roundText;

    private int currentStep;

    private List<(string label, string amount)> rewardData = new();
    private List<(TMP_Text label, TMP_Text amount)> rewardComponents = new();

    private void Start()
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
        var isModeFirstToXWins =
            MatchRules.Current.MatchWinCondition.StopCondition is MatchStopConditionType.FirstToX
            && MatchRules.Current.MatchWinCondition.WinCondition is MatchWinConditionType.Wins
            // TODO make box display able to show more than just 3 wins
            && MatchRules.Current.MatchWinCondition.AmountForStopCondition <= 3;

        roundText.text = $"Round {MatchController.Singleton.RoundCount}";

        if (isModeFirstToXWins)
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

    private int ScoreForPlayer(PlayerManager player)
    {
        return MatchRules.Current.MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins => MatchController.Singleton.WinsForPlayer(player),
            MatchWinConditionType.Kills => MatchController.Singleton.KillsForPlayer(player),
            MatchWinConditionType.Chips => player.identity.Chips,
            _ => 0,
        };
    }

    private IEnumerator AnimateScoreProgress()
    {
        wantedPoster.SetActive(false);
        progressPoster.SetActive(true);
        winProgressSection.SetActive(false);
        scoreProgressSection.SetActive(true);
        progressHeader.text = MatchRules.Current.MatchWinCondition.WinCondition.ToString();

        // Reset hint text until it is set
        hintText.text = "";

        if (MatchRules.Current.MatchWinCondition.StopCondition == MatchStopConditionType.AfterXRounds)
        {
            roundText.text = $"Round {MatchController.Singleton.RoundCount}/{MatchRules.Current.MatchWinCondition.AmountForStopCondition}";
        }

        int score = ScoreForPlayer(player);

        int scoreGain = MatchRules.Current.MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins => MatchController.Singleton.LastRound.IsWinner(player.id) ? 1 : 0,
            MatchWinConditionType.Kills => MatchController.Singleton.LastRound.KillCount(player),
            // Use player details as previous chip count, since the chip amount there is only set at round start
            MatchWinConditionType.Chips =>
                player.identity.Chips - Peer2PeerTransport.PlayerDetails.Where(p => p.id == player.id).SingleOrDefault().chips,
            _ => 0,
        };

        int previousScore = score - scoreGain;

        // TODO change this once we have more than 3 kills possible per round!
        var delayTime = MatchRules.Current.MatchWinCondition.WinCondition is MatchWinConditionType.Kills ? 0.3f : 0.1f;
        scoreText.text = FormatScore(previousScore);
        for (int i = 1; i <= scoreGain; i++)
        {
            yield return new WaitForSeconds(delayTime);
            scoreText.text = FormatScore(previousScore + i);
            scoreText.gameObject.LeanScale(1.5f * Vector3.one, delayTime).setEasePunch();
        }

        // Show hint below current score
        if (MatchRules.Current.MatchWinCondition.StopCondition == MatchStopConditionType.AfterXRounds)
        {
            int maxScore = Peer2PeerTransport.PlayerInstanceByID.Values.Max(p => ScoreForPlayer(p));
            if (score >= maxScore)
                hintText.text = "LEADER";
        }
        else
        {
            if (score >= MatchRules.Current.MatchWinCondition.AmountForStopCondition)
                hintText.text = "CAN WIN";
        }

        // TODO avoid hardcoding progress display time
        yield return new WaitForSeconds(scoreboardManager.matchProgressDelay - 1.5f);
    }

    private string FormatScore(int score)
    {
        if (MatchRules.Current.MatchWinCondition.StopCondition is MatchStopConditionType.FirstToX)
            return $"{score}/{MatchRules.Current.MatchWinCondition.AmountForStopCondition}";
        return $"{score}";
    }
}
