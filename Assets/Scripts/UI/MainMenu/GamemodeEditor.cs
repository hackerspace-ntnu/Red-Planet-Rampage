using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamemodeEditor : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown winConditionSlider;

    [SerializeField]
    private TMP_Dropdown stopConditionSlider;

    [SerializeField]
    private Slider stopConditionAmountSlider;

    [SerializeField]
    private TMP_Text stopConditionAmountLabel;

    [SerializeField]
    private Slider startingChipsSlider;

    [SerializeField]
    private Slider chipsPerRoundSlider;

    [SerializeField]
    private Slider chipsPerKillSlider;

    [SerializeField]
    private Slider chipsPerWinSlider;

    [SerializeField]
    private Slider maxChipsSlider;

    private Ruleset ruleset;

    private void Awake()
    {
        ResetRuleset();
    }

    public void ResetRuleset()
    {
        // TODO is this valid clone???
        ruleset = Instantiate(MatchRules.Singleton.CustomRulesTemplate);
        winConditionSlider.value = (int)ruleset.MatchWinCondition.WinCondition;
        stopConditionSlider.value = (int)ruleset.MatchWinCondition.StopCondition;
        stopConditionAmountSlider.value = ruleset.MatchWinCondition.AmountForStopCondition;
        startingChipsSlider.value = ruleset.StartingChips;
        chipsPerRoundSlider.value = ruleset.ChipsPerRoundPassed;
        chipsPerKillSlider.value = ruleset.ChipsPerKill;
        chipsPerWinSlider.value = ruleset.ChipsPerWin;
        maxChipsSlider.value = ruleset.MaxChips;
        ApplyCondition();
    }

    public void ApplyRuleset()
    {
        MatchRules.Singleton.Rules = Instantiate(ruleset);
    }

    private void ApplyCondition()
    {
        if (ruleset.MatchWinCondition.StopCondition is MatchStopConditionType.AfterXRounds)
        {
            stopConditionAmountLabel.text = "Rounds";
            return;
        }

        // Otherwise, display the X in First to X
        stopConditionAmountLabel.text = ruleset.MatchWinCondition.WinCondition.ToString();
    }

    public void SetWinCondition(int index)
    {
        var changed = ruleset.MatchWinCondition;
        changed.WinCondition = (MatchWinConditionType)index;
        ruleset.MatchWinCondition = changed;
        ApplyCondition();
    }

    public void SetStopCondition(int index)
    {
        var changed = ruleset.MatchWinCondition;
        changed.StopCondition = (MatchStopConditionType)index;
        ruleset.MatchWinCondition = changed;
        ApplyCondition();
    }

    public void SetRounds(float value)
    {
        var changed = ruleset.MatchWinCondition;
        changed.AmountForStopCondition = Mathf.RoundToInt(value);
        ruleset.MatchWinCondition = changed;
    }

    private void SetReward(RewardCondition condition, float value)
    {
        ruleset.Rewards = ruleset.Rewards.Where(r => r.Condition != condition).ToArray();
        var reward = new Reward
        {
            Condition = condition,
            Type = RewardType.Chips,
            Amount = Mathf.RoundToInt(value),
        };
        ruleset.Rewards = ruleset.Rewards.Append(reward).ToArray();
    }

    public void SetStartingChips(float value)
    {
        SetReward(RewardCondition.Start, value);
    }

    public void SetChipsPerRound(float value)
    {
        SetReward(RewardCondition.Survive, value);
    }

    public void SetChipsPerKill(float value)
    {
        SetReward(RewardCondition.Kill, value);
    }

    public void SetChipsPerWin(float value)
    {
        SetReward(RewardCondition.Win, value);
    }

    public void SetMaxChips(float value)
    {
        ruleset.MaxChips = Mathf.RoundToInt(value);
    }
}
