using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

[Serializable]
public struct Poster
{
    public Player player;
    public WantedPoster wantedPoster;

    public Poster(Player Player, WantedPoster WantedPoster)
    {
        player = Player;
        wantedPoster = WantedPoster;
    }
}
public class TownScoreboard : MonoBehaviour
{
    private MatchController matchController;
    private List<Player> players;

    public GameObject wantedPosterPrefab;
    private Poster[] posters;

    public GameObject crimeTextPrefab;

    [SerializeField]
    private List<string> wantedSubtitles= new List<string>();

    void Start()
    {
        matchController = MatchController.Singleton;
        players = matchController.Players;
        posters = new Poster[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            posters[i] = new Poster(players[i], Instantiate(wantedPosterPrefab, transform).GetComponentInChildren<WantedPoster>());

            posters[i].wantedPoster.HistoryValue.text = posters[i].player.playerIdentity.chips.ToString();
            posters[i].wantedPoster.Subtitle.text = wantedSubtitles[UnityEngine.Random.Range(0, wantedSubtitles.Count)];

            posters[i].wantedPoster.TotalValue.text = players[i].playerIdentity.chips.ToString();
        }

        matchController.onRoundEnd += UpdateScoreboard;
    }

    public void AddPosterCrime(Player player, string Crime, int Value)
    {
        Poster poster = GetPlayerPoster(player);
        
        GameObject crime = Instantiate(crimeTextPrefab, poster.wantedPoster.CrimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        texts[0].text = Crime;
        texts[1].text = Value.ToString();
    }

    private Poster GetPlayerPoster(Player player)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (player == posters[i].player)
                return posters[i];
        }
        Debug.LogError("Unable to find poster connected to player.");
        return posters[0];
    }

    public void UpdateScoreboard()
    {
        var lastRound = matchController.GetLastRound();
        
        foreach (var player in players)
        {
            AddPosterCrime(player, "Base reward", matchController.ChipBaseReward);
            AddPosterCrime(player, "Kills", matchController.ChipKillReward * lastRound.KillCount(player.playerManager));

            if(lastRound.IsWinner(player.playerIdentity))
            {
                AddPosterCrime(player, "Victor", matchController.ChipWinReward);
            }

            GetPlayerPoster(player).wantedPoster.TotalValue.text = player.playerIdentity.chips.ToString();

            // Animate the after battle scene
            Camera.main.GetComponent<Animator>().SetTrigger("ScoreboardZoom");

            player.playerManager.inputManager.GetComponent<Camera>().enabled = false;
        }
    }
}
