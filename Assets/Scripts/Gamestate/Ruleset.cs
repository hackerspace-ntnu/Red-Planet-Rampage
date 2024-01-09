using CollectionExtensions;
using UnityEngine;

// TODO we need a stopcondition as well, actually!
public enum MatchWinConditionType
{
    Wins,
    Score,
}

[System.Serializable]
public struct MatchWinCondition
{
    public MatchWinConditionType Type;
    public int Amount;
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
    public StartingWeaponType type;
    public Item body;
    public Item barrel;
    public Item extension;
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
    public WeaponProgressType type;
}

[CreateAssetMenu(menuName = "Ruleset")]
public class Ruleset : ScriptableObject
{
    [Header("Win Condition")]
    public MatchWinCondition MatchWinCondition;

    [Header("Round Win Condition")]
    public RoundWinCondition RoundWinCondition;

    [Header("Rewards")]
    public Reward[] Rewards;

    [Header("Weapon Progress")]
    public StartingWeapon StartingWeapon;
    public WeaponProgress WeaponProgress;

    public void Initialize()
    {
        if (StartingWeapon.type == StartingWeaponType.SharedRandom)
        {
            StartingWeapon.body = StaticInfo.Singleton.Bodies.RandomElement();
            StartingWeapon.barrel = StaticInfo.Singleton.Barrels.RandomElement();
            StartingWeapon.extension = StaticInfo.Singleton.Extensions.RandomElement();
        }
    }
}
