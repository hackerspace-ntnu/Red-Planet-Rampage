using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSelectManager : MonoBehaviour
{
    // Player 3d models in player select screen
    [SerializeField]
    private List<GameObject> playerModels;
    [SerializeField]
    private Camera playerSelectCam;
    [SerializeField]
    private List<TMP_Text> nameTags;
    [SerializeField]
    private List<TMP_Text> joinText;

    [SerializeField]
    private float minimumTime = 20f;
    [SerializeField]
    private float maximumTime = 30f;

    private PlayerInputManagerController playerInputManagerController;
    private List<TMP_Text> playerNames = new List<TMP_Text>();
    private List<Animator> playerAnimators = new List<Animator>();
    private List<string> animatorParameters = new List<string>();
    private int cardPeekCounter = 0;

    private void Awake()
    {
        for (int i = 0; i < playerModels.Count; i++)
        {
            Vector3 playerPosition = new Vector3(playerModels[i].transform.position.x, 2, playerModels[i].transform.position.z);
            joinText[i].transform.position = playerPosition;
        }
    }

    private void Start()
    {
        playerInputManagerController = PlayerInputManagerController.Singleton;
        for (int i = 0; i < playerModels.Count; i++)
        {
            playerAnimators.Add(playerModels[i].GetComponentInChildren<Animator>()); // Add all animators to list
        }

        for (int i = 0; i < playerAnimators[0].parameterCount; i++)
        {
            animatorParameters.Add(playerAnimators[0].GetParameter(i).name);
        }
        if (SteamManager.IsSteamActive)
            SteamManager.Singleton.LobbyPlayerUpdate += UpdateLobby;
    }

    public void UpdateLobby()
    {
        var i = 0;
        foreach (var player in Peer2PeerTransport.PlayerDetails)
        {
            SetupPlayerSelectModels(player.name, player.color, i);
            i++;
        }
    }

    /// <summary>
    /// Called when player is added. Sets the corresponding playermodel to active and fills in the playername in the nametag.
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="color"></param>
    /// <param name="playerID"></param>
    public void SetupPlayerSelectModels(string playerName, Color color, int playerID)
    {
        playerModels[playerID].GetComponentInChildren<SkinnedMeshRenderer>().material.color = color; // Set player model color
        playerModels[playerID].SetActive(true); // Show corresponding player model
        playerModels[playerID].transform.LookAt(new Vector3(playerSelectCam.transform.position.x, playerModels[playerID].transform.position.y, playerSelectCam.transform.position.z)); // Orient player model to look at camera
        SetPlayerNameTag(playerModels[playerID], playerName, playerID); // Create and display player nametag
        joinText[playerID].enabled = false;
    }

    /// <summary>
    /// Fills in the playername and positions it.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="playerName"></param>
    /// <param name="playerID"></param>
    public void SetPlayerNameTag(GameObject player, string playerName, int playerID)
    {
        // Check if nameTag exists already
        bool nameExists = playerNames.Any(x => x.name == playerName);

        // If it doesn't, create new nametag based on player name
        if (!nameExists)
        {
            Vector3 playerPosition = new Vector3(player.transform.position.x, 2, player.transform.position.z);

            TMP_Text name = nameTags[playerID];
            name.transform.position = playerPosition; // Sets the nametag position over the head of the player model
            name.text = playerName; // Sets the text of the nametag
            playerNames.Add(name); // Adds TMP_text object to list for monitoring
        }
    }

    /// <summary>
    /// Stops the coroutine responsible for animating players in playerSelectMenu. To be called by "Start" in main menu.
    /// </summary>
    public void PlayAnimations()
    {
        // Start random animations coroutine for playerselect
        StartCoroutine("PlayRandomAnimation");
    }

    /// <summary>
    /// Stops the coroutine responsible for animating players in playerSelectMenu. To be called by "Back" button in playerSelectMenu.
    /// </summary>
    public void StopAnimations()
    {
        // Stop random animations coroutine for playerselect
        StopCoroutine("PlayRandomAnimation");
    }

    /// <summary>
    /// Coroutine that continuously plays random idle animations in the player select menu
    /// </summary>
    IEnumerator PlayRandomAnimation()
    {
        List<string> excludeCardPeekReaction = animatorParameters;
        excludeCardPeekReaction.Remove("CardPeekReaction");

        yield return new WaitForSeconds(5f);
        while (true)
        {
            int randomAnimatorNumber = Random.Range(0, playerInputManagerController.PlayerCount); // Choose random playermodel to animate

            Animator randomAnimator = playerAnimators[randomAnimatorNumber]; // Get the animator for one of the players that has a connected input

            // If randomAnimatorNumber is player all the way to the right, don't include cardpeek trigger
            string randomTrigger = "";
            if (randomAnimatorNumber == playerInputManagerController.PlayerCount - 1)
            {
                randomTrigger = randomAnimator.GetParameter(Random.Range(2, randomAnimator.parameterCount)).name; // Choose a random trigger to set, excluding CardPeek
            }
            else if (randomAnimatorNumber == 0)
            {
                randomTrigger = excludeCardPeekReaction.RandomElement(); // Choose a random trigger to set, excluding CardPeekReaction
            }
            else
            {
                randomTrigger = randomAnimator.GetParameter(Random.Range(0, randomAnimator.parameterCount)).name; // Choose a random trigger
            }


            if ((randomTrigger == "CardPeek" || randomTrigger == "CardPeekReaction") && (playerInputManagerController.PlayerCount > 1) && (cardPeekCounter == 0))
            {
                randomAnimator.SetTrigger("CardPeek");
                playerAnimators[randomAnimatorNumber + 1].SetTrigger("CardPeekReaction");

                cardPeekCounter++; // Increment counter to make sure this animation isn't played twice in a row
            }
            else if (randomTrigger == "CardPeek" || randomTrigger == "CardPeekReaction") // Choose new animation to play if cardpeek or cardpeekreaction is chosen a second time
            {
                randomTrigger = randomAnimator.GetParameter(Random.Range(2, randomAnimator.parameterCount - 1)).name;

                cardPeekCounter = 0;
                randomAnimator.SetTrigger(randomTrigger);
            }
            else
            {
                cardPeekCounter = 0;
                randomAnimator.SetTrigger(randomTrigger);
            }
            yield return new WaitForSeconds(Random.Range(minimumTime, maximumTime));
        }
    }

    private void OnDestroy()
    {
        if (SteamManager.IsSteamActive)
            SteamManager.Singleton.LobbyPlayerUpdate -= UpdateLobby;
    }
}
