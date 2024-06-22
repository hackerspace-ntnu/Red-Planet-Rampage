using System.Collections.Generic;
using System.Linq;
using CollectionExtensions;
using UnityEngine;

public enum MatchWinConditionType
{
    Kills,
    Wins,
    Score,
}

public enum MatchStopConditionType
{
    AfterXRounds,
    FirstToXWins,
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
    IndividualRandom,
    SharedRandom,
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

[CreateAssetMenu(menuName = "Ruleset")]
public class Ruleset : ScriptableObject
{
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

    public void Initialize()
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
            return AuctionType.Random;
        return AuctionProgress[index].Type;
    }

    public bool IsMatchOver(List<Round> rounds, uint currentWinner) =>
         MatchWinCondition.StopCondition switch
         {
             MatchStopConditionType.FirstToXWins => rounds.Where(r => r.IsWinner(currentWinner)).Count() >= MatchWinCondition.AmountForStopCondition,
             MatchStopConditionType.AfterXRounds => rounds.Count >= MatchWinCondition.AmountForStopCondition,
             _ => true,
         };

    public uint DetermineWinner(List<Round> rounds) =>
        MatchWinCondition.WinCondition switch
        {
            MatchWinConditionType.Wins => WinnerByWins(rounds),
            MatchWinConditionType.Kills => WinnerByKills(rounds),
            _ => 0,
        };

    private uint WinnerByWins(List<Round> rounds) =>
        Peer2PeerTransport.PlayerDetails.MaxBy(p => rounds.Where(r => r.IsWinner(p.id)).Count()).id;

    private uint WinnerByKills(List<Round> rounds) =>
        Peer2PeerTransport.PlayerDetails.MaxBy(p => rounds.Sum(r => r.KillCount(p.id))).id;

    public NetworkRuleset ToNetworkRuleset() =>
        new NetworkRuleset
        {
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
