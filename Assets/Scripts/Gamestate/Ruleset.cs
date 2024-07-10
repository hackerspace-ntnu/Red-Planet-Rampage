using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CollectionExtensions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum MatchWinConditionType
{
    Kills,
    Wins,
    Chips,
}

public enum MatchStopConditionType
{
    AfterXRounds,
    FirstToX,
}

[System.Serializable]
public struct MatchWinCondition
{
    public MatchWinConditionType WinCondition;
    public MatchStopConditionType StopCondition;
    public int AmountForStopCondition;
}

public enum RoundType
{
    LastManStanding,
}

[System.Serializable]
public struct RoundWinCondition
{
    // TODO respawning could be added here?
    public RoundType Type;
    public int Amount;
}

public enum RewardCondition
{
    Start,
    Survive,
    Kill,
    Win,
}

public enum RewardType
{
    Chips,
    Score,
}

[System.Serializable]
public struct Reward
{
    public RewardCondition Condition;
    public RewardType Type;
    public int Amount;
}

public enum StartingWeaponType
{
    Standard,
    Specific,
    SharedRandom,
    IndividualRandom,
}

[System.Serializable]
public struct StartingWeapon
{
    public StartingWeaponType Type;
    public Item Body;
    public Item Barrel;
    public Item Extension;

    public NetworkStartingWeapon ToNetworkStartingWeapon() =>
        new NetworkStartingWeapon
        {
            Type = Type,
            Body = Body.id,
            Barrel = Barrel.id,
            Extension = Extension ? Extension.id : "None",
        };
}

[System.Serializable]
public struct NetworkStartingWeapon
{
    public StartingWeaponType Type;
    public string Body;
    public string Barrel;
    public string Extension;

    public StartingWeapon ToStartingWeapon() =>
        new StartingWeapon
        {
            Type = Type,
            Body = StaticInfo.Singleton.ItemsById[Body],
            Barrel = StaticInfo.Singleton.ItemsById[Barrel],
            Extension = Extension != null && Extension != "None" ? StaticInfo.Singleton.ItemsById[Extension] : null,
        };
}

public enum WeaponProgressType
{
    Auction,
    GunGame,
    None,
}

[System.Serializable]
public struct WeaponProgress
{
    public WeaponProgressType Type;
}

// TODO implement auction!
public enum AuctionType
{
    Body,
    Barrel,
    Extension,
    OneOfEach,
    Random,
}

[System.Serializable]
public struct Auction
{
    public AuctionType Type;
}

[System.Serializable]
public struct NetworkRuleset
{
    public GameModeVariant GameMode;
    public MatchWinCondition MatchWinCondition;

    public RoundWinCondition RoundWinCondition;
    public float RoundLength;

    public Reward[] Rewards;
    public int MaxChips;

    public NetworkStartingWeapon StartingWeapon;
    public WeaponProgress WeaponProgress;
    public Auction[] AuctionProgress;

    public Ruleset ToRuleset() =>
        new Ruleset
        {
            GameMode = GameMode,
            MatchWinCondition = MatchWinCondition,
            RoundWinCondition = RoundWinCondition,
            RoundLength = RoundLength,
            Rewards = Rewards,
            MaxChips = MaxChips,
            StartingWeapon = StartingWeapon.ToStartingWeapon(),
            WeaponProgress = WeaponProgress,
            AuctionProgress = AuctionProgress
        };
}

public enum GameModeVariant
{
    Custom,
    FirstTo30Chips,
    SixRounds,
    ThreeStrikes,
}

[CreateAssetMenu(menuName = "Ruleset")]
public class Ruleset : ScriptableObject
{
    [Header("Fluff")]
    [SerializeField]
    private string displayName;
    public string DisplayName => displayName;

    [TextArea]
    [SerializeField]
    private string description;
    public string Description => description;

    [SerializeField]
    [FormerlySerializedAs("gameMode")]
    public GameModeVariant GameMode = GameModeVariant.Custom;

    [Header("Win Condition")]
    public MatchWinCondition MatchWinCondition;

    [Header("Round Win Condition")]
    public RoundWinCondition RoundWinCondition;
    public float RoundLength = 180;

    [Header("Rewards")]
    public Reward[] Rewards;
    public int MaxChips = 20;

    [Header("Weapon Progress")]
    public StartingWeapon StartingWeapon;
    public WeaponProgress WeaponProgress;
    public Auction[] AuctionProgress;

    public int StartingChips => Rewards.Where(r => r.Condition == RewardCondition.Start && r.Type == RewardType.Chips).Sum(r => r.Amount);
    public int ChipsPerRoundPassed => Rewards.Where(r => r.Condition == RewardCondition.Survive && r.Type == RewardType.Chips).Sum(r => r.Amount);
    public int ChipsPerKill => Rewards.Where(r => r.Condition == RewardCondition.Kill && r.Type == RewardType.Chips).Sum(r => r.Amount);
    public int ChipsPerWin => Rewards.Where(r => r.Condition == RewardCondition.Win && r.Type == RewardType.Chips).Sum(r => r.Amount);

    public void InitializeRulesAfterCreation()
    {
        var editedStartingWeapon = StartingWeapon;
        switch (StartingWeapon.Type)
        {
            case StartingWeaponType.Standard:
                {
                    editedStartingWeapon.Body = StaticInfo.Singleton.StartingBody;
                    editedStartingWeapon.Barrel = StaticInfo.Singleton.StartingBarrel;
                    editedStartingWeapon.Extension = StaticInfo.Singleton.StartingExtension;
                    break;
                }
            case StartingWeaponType.SharedRandom:
                {
                    editedStartingWeapon.Body = StaticInfo.Singleton.Bodies.RandomElement();
                    editedStartingWeapon.Barrel = StaticInfo.Singleton.Barrels.RandomElement();
                    editedStartingWeapon.Extension = StaticInfo.Singleton.Extensions.RandomElement();
                    break;
                }
            default:
                break;
        }
        StartingWeapon = editedStartingWeapon;
    }

    public AuctionType AuctionForRound(int round)
    {
        var index = round - 1;
        if (index < 0 || index >= AuctionProgress.Length)
            return AuctionType.OneOfEach;
        return AuctionProgress[index].Type;
    }

    #region Winning
    public bool IsMatchOver(List<Round> rounds, uint currentWinner) =>
        MatchWinCondition.StopCondition switch
        {
            MatchStopConditionType.FirstToX => CandidatesForFirstToX(rounds).Any(p => p.id == currentWinner),
            MatchStopConditionType.AfterXRounds =>
                rounds.Count >= MatchWinCondition.AmountForStopCondition
                && IsWinnerForAfterXRoundsDetermined(rounds, currentWinner),
            _ => true,
        };

    private bool IsWinnerForAfterXRoundsDetermined(List<Round> rounds, uint currentWinner)
    {
        var candidates = CandidatesForLastXRounds(rounds).ToList();
        return candidates.Count == 1 || candidates.Any(p => p.id == currentWinner);
    }

    /// <summary>
    /// Assumes that IsMatchOver() is true, call that one first
    /// </summary>
    public uint DetermineWinner(List<Round> rounds, uint currentWinner) =>
        MatchWinCondition.StopCondition switch
        {
            MatchStopConditionType.FirstToX => currentWinner,
            MatchStopConditionType.AfterXRounds => WinnerForAfterXRounds(rounds, currentWinner),
            _ => 0,
        };

    private uint WinnerForAfterXRounds(List<Round> rounds, uint currentWinner)
    {
        var candidates = CandidatesForLastXRounds(rounds).ToList();
        if (candidates.Count == 1)
            return candidates.Single().id;
        else
            return candidates.Single(p => p.id == currentWinner).id;
    }

    private IEnumerable<PlayerDetails> CandidatesForFirstToX(List<Round> rounds) =>
        MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins =>
                Peer2PeerTransport.PlayerDetails.Where(p => rounds.Where(r => r.IsWinner(p.id)).Count() >= MatchWinCondition.AmountForStopCondition),
            MatchWinConditionType.Kills =>
                Peer2PeerTransport.PlayerDetails.Where(p => rounds.Sum(r => r.KillCount(p.id)) >= MatchWinCondition.AmountForStopCondition),
            MatchWinConditionType.Chips =>
                Peer2PeerTransport.PlayerDetails.Where(p => Peer2PeerTransport.PlayerInstanceByID[p.id].identity.Chips >= MatchWinCondition.AmountForStopCondition),
            _ => Peer2PeerTransport.PlayerDetails,
        };

    private IEnumerable<PlayerDetails> CandidatesForLastXRounds(List<Round> rounds)
    {
        var max = MaxForWinCondition(rounds);
        return MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins =>
                Peer2PeerTransport.PlayerDetails.Where(p => rounds.Where(r => r.IsWinner(p.id)).Count() == max),
            MatchWinConditionType.Kills =>
                Peer2PeerTransport.PlayerDetails.Where(p => rounds.Sum(r => r.KillCount(p.id)) == max),
            MatchWinConditionType.Chips =>
                Peer2PeerTransport.PlayerDetails.Where(p => Peer2PeerTransport.PlayerInstanceByID[p.id].identity.Chips == max),
            _ => Peer2PeerTransport.PlayerDetails,
        };
    }

    private int MaxForWinCondition(List<Round> rounds) =>
        MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins =>
                Peer2PeerTransport.PlayerDetails.Max(p => rounds.Where(r => r.IsWinner(p.id)).Count()),
            MatchWinConditionType.Kills =>
                Peer2PeerTransport.PlayerDetails.Max(p => rounds.Sum(r => r.KillCount(p.id))),
            MatchWinConditionType.Chips =>
                Peer2PeerTransport.PlayerDetails.Max(p => Peer2PeerTransport.PlayerInstanceByID[p.id].identity.Chips),
            _ => 0
        };
    #endregion

    public NetworkRuleset ToNetworkRuleset() =>
        new NetworkRuleset
        {
            GameMode = GameMode,
            MatchWinCondition = MatchWinCondition,
            RoundWinCondition = RoundWinCondition,
            RoundLength = RoundLength,
            Rewards = Rewards,
            MaxChips = MaxChips,
            StartingWeapon = StartingWeapon.ToNetworkStartingWeapon(),
            WeaponProgress = WeaponProgress,
            AuctionProgress = AuctionProgress
        };
}
