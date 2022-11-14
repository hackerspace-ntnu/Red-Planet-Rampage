using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Round
{
    private PlayerManager winner;
    public PlayerManager Winner => winner;

    private Dictionary<PlayerManager, List<PlayerManager>> kills = new Dictionary<PlayerManager, List<PlayerManager>>();
    public ReadOnlyDictionary<PlayerManager, ReadOnlyCollection<PlayerManager>> Kills =>
            new ReadOnlyDictionary<PlayerManager, ReadOnlyCollection<PlayerManager>>(
                kills.ToDictionary(kill => kill.Key, kill => new ReadOnlyCollection<PlayerManager>(kill.Value))
            );

    private List<PlayerManager> players = new List<PlayerManager>();
    public ReadOnlyCollection<PlayerManager> Players => new ReadOnlyCollection<PlayerManager>(players);

    private List<PlayerManager> livingPlayers = new List<PlayerManager>();
    public ReadOnlyCollection<PlayerManager> LivingPlayers => new ReadOnlyCollection<PlayerManager>(livingPlayers);

    public Round(List<PlayerManager> players)
    {
        this.players = players;
        livingPlayers = players.ToList();
        foreach (var player in players)
        {
            player.onDeath += OnDeath;
        }
        MatchController.Singleton.onRoundEnd += OnRoundEnd;
    }

    public int KillCount(PlayerManager player)
    {
        if (kills.TryGetValue(player, out var playerKills))
            return playerKills.Count;
        return 0;
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
        if (kills.ContainsKey(killer))
        {
            kills[killer].Add(victim);
        }
        else
        {
            List<PlayerManager> playerKills = new List<PlayerManager>();
            playerKills.Add(victim);
            kills.Add(killer, playerKills);
        }

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
