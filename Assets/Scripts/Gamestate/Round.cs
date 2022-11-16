using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Round
{
    private PlayerManager winner;
    public PlayerManager Winner => winner;

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
    private readonly ReadOnlyDictionary<PlayerManager, List<PlayerManager>> kills;
    public readonly ReadOnlyDictionary<PlayerManager, ReadOnlyCollection<PlayerManager>> Kills;

    private readonly List<PlayerManager> players;
    public readonly ReadOnlyCollection<PlayerManager> Players;

    private readonly List<PlayerManager> livingPlayers = new List<PlayerManager>();
    public readonly ReadOnlyCollection<PlayerManager> LivingPlayers;

    public Round(IEnumerable<PlayerManager> roundPlayers)
    {
        players = new List<PlayerManager>(roundPlayers);
        livingPlayers = new List<PlayerManager>(roundPlayers);
        kills = new ReadOnlyDictionary<PlayerManager, List<PlayerManager>>(players.ToDictionary(
            /*key   */ player => player,
            /*value */ player => new List<PlayerManager>()
        ));

        Players = new ReadOnlyCollection<PlayerManager>(this.players);
        LivingPlayers = new ReadOnlyCollection<PlayerManager>(livingPlayers);
        Kills = new ReadOnlyDictionary<PlayerManager, ReadOnlyCollection<PlayerManager>>(players.ToDictionary(
            /*key   */ player => player,
            /*value */ player => new ReadOnlyCollection<PlayerManager>(kills[player]) 
        ));

        foreach (var player in players)
        {
            player.onDeath += OnDeath;
        }
        MatchController.Singleton.onRoundEnd += OnRoundEnd;
    }

    public int KillCount(PlayerManager player)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(player), "player not registered in start of round!", player);
#endif
        return kills[player].Count;
    }

    public bool IsWinner(PlayerManager player)
    {
        return player == winner;
    }

    public void OnRoundEnd()
    {
        foreach (var player in players)
        {
            player.onDeath -= OnDeath;
        }
        MatchController.Singleton.onRoundEnd -= OnRoundEnd;
    }

    //TODO: Create struct for damagecontext with info about who was killed as parameter instead
    // (Currently waiting for damage/basic gunplay)
    private void OnDeath(PlayerManager killer, PlayerManager victim)
    {
#if DEBUG
        Debug.Assert(kills.ContainsKey(killer), "killer not registered in start of round!", killer);
#endif
        kills[killer].Add(victim);
        livingPlayers.Remove(victim);

        CheckWinCondition(killer);
    }

    private void CheckWinCondition(PlayerManager lastKiller)
    {
        if (livingPlayers.Count < 2)
        {
            winner = lastKiller;
            MatchController.Singleton.EndActiveRound();
        }
    }
}
