using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Wrapper struct for tying refference to Player class with the in-game player.
/// </summary>
public struct MatchPlayer
{
    public MatchPlayer(Player player, PlayerStateController playerStateController)
    {
        this.player = player;
        this.playerStateController = playerStateController;
        chips = 0;
    }
    // Reference to player identity class
    public Player player;
    // Reference to in-match player
    public PlayerStateController playerStateController;
    // Currency
    public int chips;
}

public class MatchController : MonoBehaviour
{
    public static MatchController Singleton { get; private set; }

    public delegate void MatchEvent();

    public MatchEvent onRoundEnd;
    public MatchEvent onRoundStart;
    public MatchEvent onBiddingStart;
    public MatchEvent onBiddingEnd;

    private List<MatchPlayer> matchPlayers = new List<MatchPlayer>();
    private List<Round> rounds = new List<Round>();

    void Start()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        PlayerInputManagerController.Singleton.playerInputs.ForEach(playerInput =>
        {
            var player = playerInput.gameObject.GetComponent<Player>();
            var playerStateController = playerInput.gameObject.transform.parent.GetComponent<PlayerStateController>();
            matchPlayers.Add(new MatchPlayer(player, playerStateController));
        });

        // TODO do something else funky wunky
        StartNextRound();
    }

    public void StartNextRound()
    {
        onRoundStart?.Invoke();
        rounds.Add(new Round(matchPlayers.Select(player => player.playerStateController).ToList()));
    }

    public void EndActiveRound()
    {
        onRoundEnd?.Invoke();
        print("aaaaand the winner issssss" + rounds.Last().winner);
        SceneManager.LoadScene("Menu");
    }
}
