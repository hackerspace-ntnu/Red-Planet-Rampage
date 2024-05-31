using CollectionExtensions;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    private List<Animator> playerAnimators = new List<Animator>();
    private List<string> animatorParameters = new List<string>();
    private int cardPeekCounter = 0;

    private void Awake()
    {
        for (int i = 0; i < playerModels.Count; i++)
        {
            Vector3 playerPosition = new Vector3(playerModels[i].transform.position.x, 2, playerModels[i].transform.position.z);
            joinText[i].transform.localPosition = Vector3.zero;
            nameTags[i].transform.position = playerPosition;
        }
    }

    private void Start()
    {
        playerInputManagerController = PlayerInputManagerController.Singleton;

        // Find animators and their parameters
        foreach (var t in playerModels)
        {
            playerAnimators.Add(t.GetComponentInChildren<Animator>());
        }
        foreach (var parameter in playerAnimators[0].parameters)
        {
            animatorParameters.Add(parameter.name);
        }

        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRecieved += UpdateLobby;
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRemoved += UpdateLobby;
    }

    private void OnDestroy()
    {
        if (NetworkManager.singleton)
        {
            ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRecieved -= UpdateLobby;
            ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRemoved -= UpdateLobby;
        }
    }

    public void UpdateLobby()
    {
        var i = 0;
        foreach (var player in Peer2PeerTransport.PlayerDetails)
        {
            SetupPlayerModel(player, i);
            i++;
        }
        for (; i < playerModels.Count; i++)
        {
            DisablePlayerModel(i);
        }
    }

    private void UpdateLobby(PlayerDetails details)
    {
        UpdateLobby();
    }

    /// <summary>
    /// Called when player is added. Sets the corresponding playermodel to active and fills in the playername in the nametag.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="index"></param>
    public void SetupPlayerModel(PlayerDetails player, int index)
    {
        playerModels[index].GetComponentInChildren<SkinnedMeshRenderer>().material.color = player.color; // Set player model color
        playerModels[index].SetActive(true); // Show corresponding player model
        playerModels[index].transform.LookAt(new Vector3(playerSelectCam.transform.position.x, playerModels[index].transform.position.y, playerSelectCam.transform.position.z)); // Orient player model to look at camera
        nameTags[index].text = Peer2PeerTransport.PlayerNameWithIndex(player);
        nameTags[index].enabled = true;
        joinText[index].enabled = false;
    }

    /// <summary>
    /// Called when player is removed. Stops displaying the affected player.
    /// </summary>
    /// <param name="playerID"></param>
    public void DisablePlayerModel(int playerID)
    {
        playerModels[playerID].SetActive(false);
        nameTags[playerID].enabled = false;
        joinText[playerID].enabled = true;
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
}
