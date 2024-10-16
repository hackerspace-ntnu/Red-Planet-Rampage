using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public struct PlayerStats
{
    public uint player;
    public uint[] kills;
}

public struct NetworkRound
{
    public uint? winner;
    public PlayerStats[] stats;
}


public class Round
{
    private uint? winner;
    public uint? Winner => winner;

    /* Readonly Terminology
     *
     * readonly
     * attribute makes the member field only assignable during constructor call.
     *
     * ReadOnlyDictionary
     * is a "view" (i.e. looking at same underlying values and space in memory as the dictionary it's wrapping)
     *      Using it ensures that we cannot add new entries to the dictionary. (i.e. adding new keys)
     *      It does not however restrict us changing the values pointed to by keys.
     *
     *      Simply put:
     *          a ReadOnlyDictionary<int, List<int>> allows for changing the lists the existing integer keys point to
     *
     * ReadOnlyCollection
     * is a "view" (i.e. looking at same underlying values and space in memory as the collection it's wrapping)
     *      Using it ensures we cannot add new entries to the collection.
     *      It does not however restrict us changing the values pointed to by the references held by the collection.
     *
     *      Simply put:
     *          a ReadOnlyCollection<int> cannot have its entries changed, as they are value types.
     *          a ReadOnlyCollection<PlayerManager> cannot have new or different entries,
     *              but the fields of each PlayerManager in the collection can change!
     */


    //Number of players never change after the initialisation of the dictionary, hence ReadOnlyDictionary
    private readonly ReadOnlyDictionary<uint, List<uint>> kills;
    public readonly ReadOnlyDictionary<uint, ReadOnlyCollection<uint>> Kills;

    private readonly List<uint> players;
    public readonly ReadOnlyCollection<uint> Players;

    private readonly List<uint> livingPlayers;
    public readonly ReadOnlyCollection<uint> LivingPlayers;

    // Note that this list may not contain recognizable data in future rounds
    private readonly List<PlayerManager> playerManagers;

    private readonly List<DamageInfo> damageThisFrame = new();

    private readonly RoundWinCondition winCondition;

    public Round(IEnumerable<PlayerManager> roundPlayers)
    {
        winCondition = MatchRules.Singleton.Rules.RoundWinCondition;
        var ids = roundPlayers.Select(p => p.id);
        playerManagers = roundPlayers.ToList();
        players = ids.ToList();
        livingPlayers = ids.ToList();
        kills = new ReadOnlyDictionary<uint, List<uint>>(ids.ToDictionary(
            /*key   */ id => id,
            /*value */ id => new List<uint>()
        ));

        Players = new ReadOnlyCollection<uint>(this.players);
        LivingPlayers = new ReadOnlyCollection<uint>(livingPlayers);
        Kills = new ReadOnlyDictionary<uint, ReadOnlyCollection<uint>>(ids.ToDictionary(
            /*key   */ id => id,
            /*value */ id => new ReadOnlyCollection<uint>(kills[id])
        ));

        foreach (var player in roundPlayers)
        {
            player.onDeath += OnDeath;
        }

        MatchController.Singleton.onOutcomeDecided += OnOutcomeDecided;
    }

    public int KillCount(uint id)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(id), $"Player {id} not registered in round statistics!");
#endif
        return kills[id].Count;
    }

    public int KillCount(PlayerManager player)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(player.id), "Player not registered in round statistics!", player);
#endif
        return kills[player.id].Count;
    }

    public bool IsWinner(PlayerIdentity player)
    {
        return winner is not null && player.id == winner;
    }

    public bool IsWinner(uint id)
    {
        return winner is not null && id == winner;
    }

    private void OnOutcomeDecided()
    {
        foreach (var player in playerManagers)
        {
            player.onDeath -= OnDeath;
        }

        MatchController.Singleton.onOutcomeDecided -= OnOutcomeDecided;
    }

    private void OnDeath(PlayerManager killer, PlayerManager victim, DamageInfo info)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(killer.id), "killer not registered in start of round!", killer);
#endif
        livingPlayers.Remove(victim.id);
        damageThisFrame.Add(info);

        if (livingPlayers.Count == 2)
        {
            MusicTrackManager.Singleton.IntensifyBattleTheme();
        }

        // Only register a kill if it wasn't a suicide
        if (killer == victim)
            return;

        killer.onKill?.Invoke(killer, victim, info);
        kills[killer.id].Add(victim.id);
        PersistentInfo.RegisterKill(killer.identity);

        // TODO Add theoretical chips from win?
        var isFirstTo30Chips = MatchRules.Current.GameMode is GameModeVariant.FirstTo30Chips;
        var isAboutToReachChipLimit = victim.identity.Chips >= MatchRules.Current.MatchWinCondition.AmountForStopCondition;
        var isKillerLocalPlayer = Peer2PeerTransport.LocalPlayerInstances.Any(p => p.id == killer.id);
        if (isFirstTo30Chips && isAboutToReachChipLimit && isKillerLocalPlayer)
            SteamManager.Singleton.UnlockAchievement(AchievementType.Clutch);
    }

    /// <summary>
    /// Only called in LateUpdate in MatchController
    /// </summary>
    public bool CheckWinCondition()
    {
        var lessThanTwoPlayersLeft = livingPlayers.Count < 2;
        if (lessThanTwoPlayersLeft)
        {
            winner = DetermineWinner();
        }

        damageThisFrame.Clear();
        return lessThanTwoPlayersLeft;
    }

    private uint? DetermineWinner()
    {
        if (livingPlayers.Count == 0 && damageThisFrame.Count > 0)
        {
            // Determine who fired the shot that killed 'em all
            return damageThisFrame.Where(d => d.sourcePlayer is not null).Select(d => d.sourcePlayer.id)
                .First();
        }

        // We have a survivor
        return livingPlayers.First();
    }

    public NetworkRound SummarizeRound() =>
        new()
        {
            winner = winner,
            stats = kills.Select(pair => new PlayerStats
            {
                player = pair.Key,
                kills = pair.Value.ToArray()
            }
            ).ToArray()
        };

    public void UpdateFromSummary(NetworkRound summary)
    {
        winner = summary.winner;
        foreach (var stats in summary.stats)
        {
            kills[stats.player].Clear();
            kills[stats.player].AddRange(stats.kills);
        }
    }
}
