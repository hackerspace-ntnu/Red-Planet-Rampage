using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

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
        onRoundEnd += () => Debug.Log("End of round " + rounds.Count());
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

        // TODO assign chips

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        var lastWinner = rounds.Last().winner;
        var wins = rounds.Where(round => round.winner == lastWinner).Count();
        Debug.Log("Current winner (" + lastWinner.ToString() + ") has " + wins + " wins.");
        if (wins >= 3)
        {
            // We have a winner!
            // TODO go to announcer thingy
            Debug.Log("Aaaaand the winner iiiiiiiis " + lastWinner.ToString());

            // Update playerInputs in preperation for Menu scene
            PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
            foreach (PlayerInput inputs in PlayerInputManagerController.Singleton.playerInputs)
            {
                // Update listeners to new map
                inputs.GetComponent<InputManager>().RemoveListeners();
                inputs.GetComponent<InputManager>().AddListeners();
                // Free the playerInputs from their mortail coils (Player prefab)
                inputs.transform.parent = null;
                DontDestroyOnLoad(inputs);
            }

            SceneManager.LoadScene("Menu");
        }
        else
        {
            // TODO Go to bidding round first!
            StartNextRound();
        }
    }
}
