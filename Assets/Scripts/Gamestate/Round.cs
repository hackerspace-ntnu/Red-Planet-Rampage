using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Round
{
    private uint winner;
    public uint Winner => winner;

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

    public Round(IEnumerable<PlayerManager> roundPlayers)
    {
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

    public int KillCount(PlayerManager player)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(player.id), "Player not registered in round statistics!", player);
#endif
        return kills[player.id].Count;
    }

    public bool IsWinner(PlayerIdentity player)
    {
        return player.id == Winner;
    }

    public bool IsWinner(uint id)
    {
        return id == Winner;
    }

    public void OnOutcomeDecided()
    {
        foreach (var player in playerManagers)
        {
            player.onDeath -= OnDeath;
        }
        MatchController.Singleton.onOutcomeDecided -= OnOutcomeDecided;
    }

    // TODO Fix edgecases where one player sets off an explosion that kills both itself and another player.
    //      Due to nondeterministic execution order differences, we should wait until the next fram to determine a winner.
    private void OnDeath(PlayerManager killer, PlayerManager victim, DamageInfo info)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(killer.id), "killer not registered in start of round!", killer);
#endif
        livingPlayers.Remove(victim.id);

        if (livingPlayers.Count == 2)
        {
            MusicTrackManager.Singleton.IntensifyBattleTheme();
        }

        // Only register a kill if it wasn't a suicide
        if (killer != victim)
        {
            kills[killer.id].Add(victim.id);
            PersistentInfo.RegisterKill(killer.identity);
            CheckWinCondition();
        }
        // If it was a suicide, we should give the surviving player the win if there's only one
        else if (livingPlayers.Count < 2)
        {
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (livingPlayers.Count < 2)
        {
            winner = livingPlayers.FirstOrDefault();
            MatchController.Singleton.EndActiveRound();
        }
    }
}
