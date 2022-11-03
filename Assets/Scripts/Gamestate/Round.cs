using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Round
{
    public PlayerStateController winner;
    public Dictionary<PlayerStateController, List<PlayerStateController>> kills = new Dictionary<PlayerStateController, List<PlayerStateController>>();
    public List<PlayerStateController> players = new List<PlayerStateController>();
    public List<PlayerStateController> livingPlayers = new List<PlayerStateController>();

    public Round(List<PlayerStateController> players)
    {
        this.players = players;
        livingPlayers = players.ToList();
        foreach (var player in players)
        {
            player.onDeath += OnDeath;
        }
        MatchController.Singleton.onRoundEnd += OnRoundEnd;
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
    private void OnDeath(PlayerStateController killer, PlayerStateController victim)
    {
        if (kills.ContainsKey(killer))
        {
            kills[killer].Add(victim);
        }
        else
        {
            List<PlayerStateController> playerKills = new List<PlayerStateController>();
            playerKills.Add(victim);
            kills.Add(killer, playerKills);
        }

        livingPlayers.Remove(victim);

        CheckWinCondition(killer);
    }

    private void CheckWinCondition(PlayerStateController lastKiller)
    {
        if (livingPlayers.Count < 2)
        {
            winner = lastKiller;
            MatchController.Singleton.EndActiveRound();
        }
    }
}
