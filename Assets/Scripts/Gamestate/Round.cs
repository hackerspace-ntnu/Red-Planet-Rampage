using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Round
{
    public PlayerStateController winner;
    public Dictionary<PlayerStateController, List<PlayerStateController>> kills = new Dictionary<PlayerStateController, List<PlayerStateController>>();
    public List<PlayerStateController> livingPlayers = new List<PlayerStateController>();

    public Round(List<PlayerStateController> players)
    {
        livingPlayers = players;
        foreach (var player in players)
        {
            player.onDeath += OnDeath;
        }
    }

    public void OnRoundEnd()
    {
        foreach (var player in livingPlayers)
        {
            player.onDeath -= OnDeath;
        }
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

        CheckWinState(killer);
    }

    private void CheckWinState(PlayerStateController lastKiller)
    {
        if (livingPlayers.Count < 2)
        {
            winner = lastKiller;
            MatchController.Singleton.EndActiveRound();
        }
    }
}
