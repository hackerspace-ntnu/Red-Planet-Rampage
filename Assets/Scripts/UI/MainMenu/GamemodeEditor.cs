using System;
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

    [SerializeField]
    private TMP_Dropdown startingWeaponDropdown;

    [SerializeField]
    private TMP_Dropdown startingBodyDropdown;

    [SerializeField]
    private TMP_Dropdown startingBarrelDropdown;

    [SerializeField]
    private TMP_Dropdown startingExtensionDropdown;

    private Ruleset ruleset;

    private void Awake()
    {
        AddDropdownValues();
        ResetRuleset();
    }

    private void AddDropdownValues()
    {
        startingBodyDropdown.AddOptions(StaticInfo.Singleton.Bodies.Select(b => b.displayName).ToList());
        startingBarrelDropdown.AddOptions(StaticInfo.Singleton.Barrels.Select(b => b.displayName).ToList());
        startingExtensionDropdown.AddOptions(new string[] { "None" }.Union(StaticInfo.Singleton.Extensions.Select(b => b.displayName)).ToList());
        startingBodyDropdown.RefreshShownValue();
        startingBarrelDropdown.RefreshShownValue();
        startingExtensionDropdown.RefreshShownValue();
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

        startingWeaponDropdown.value = (int)ruleset.StartingWeapon.Type;
        startingBodyDropdown.value = StaticInfo.Singleton.Bodies.IndexOf(i => i == ruleset.StartingWeapon.Body);
        startingBarrelDropdown.value = StaticInfo.Singleton.Barrels.IndexOf(i => i == ruleset.StartingWeapon.Barrel);
        // -1 should become 0 for the special extension index
        startingExtensionDropdown.value = 1 + StaticInfo.Singleton.Extensions.IndexOf(i => i == ruleset.StartingWeapon.Extension);

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

    public void SetStartingWeaponType(int index)
    {
        var changed = ruleset.StartingWeapon;
        changed.Type = (StartingWeaponType)index;
        ruleset.StartingWeapon = changed;

        var shouldShowWeaponDropdowns = ruleset.StartingWeapon.Type is StartingWeaponType.Specific;
        startingBodyDropdown.transform.parent.gameObject.SetActive(shouldShowWeaponDropdowns);
        startingBarrelDropdown.transform.parent.gameObject.SetActive(shouldShowWeaponDropdowns);
        startingExtensionDropdown.transform.parent.gameObject.SetActive(shouldShowWeaponDropdowns);
    }

    public void SetStartingBody(int index)
    {
        var changed = ruleset.StartingWeapon;
        changed.Body = StaticInfo.Singleton.Bodies[index];
        ruleset.StartingWeapon = changed;
    }

    public void SetStartingBarrel(int index)
    {
        var changed = ruleset.StartingWeapon;
        changed.Barrel = StaticInfo.Singleton.Barrels[index];
        ruleset.StartingWeapon = changed;
    }

    public void SetStartingExtension(int index)
    {
        var changed = ruleset.StartingWeapon;
        // Extension "index" starts at 0 for no extension
        changed.Extension = index == 0 ? null : StaticInfo.Singleton.Extensions[index - 1];
        ruleset.StartingWeapon = changed;
    }
}
