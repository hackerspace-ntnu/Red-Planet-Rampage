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
    private float minimumTime = 20f;
    [SerializeField]
    private float maximumTime = 30f;

    private PlayerInputManagerController playerInputManagerController;
    private List<TMP_Text> playerNames;
    private List<Animator> playerAnimators;
    private int cardPeekCounter = 0;
    private bool animationPlaying = false;

    public void Start()
    {
        playerInputManagerController = PlayerInputManagerController.Singleton;
        playerNames = new List<TMP_Text>();
        playerAnimators = new List<Animator>();

        for (int i = 0; i < playerModels.Count; i++)
        {
            playerAnimators.Add(playerModels[i].GetComponentInChildren<Animator>()); // Add all animators to list
        }
    }

    /// <summary>
    /// Called when player is added. Sets the corresponding playermodel to active and fills in the playername in the nametag.
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="color"></param>
    /// <param name="playerID"></param>
    public void setupPlayerSelectModels(string playerName, Color color, int playerID)
    {
        playerModels[playerID].GetComponentInChildren<SkinnedMeshRenderer>().material.color = color; // Set player model color
        playerModels[playerID].SetActive(true); // Show corresponding player model
        playerModels[playerID].transform.LookAt(new Vector3(playerSelectCam.transform.position.x, playerModels[playerID].transform.position.y, playerSelectCam.transform.position.z)); // Orient player model to look at camera
        setPlayerNameTag(playerModels[playerID], playerName, playerID); // Create and display player nametag
    }

    /// <summary>
    /// Fills in the playername and positions it.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="playerName"></param>
    /// <param name="playerID"></param>
    public void setPlayerNameTag(GameObject player, string playerName, int playerID)
    {
        // Check if nameTag exists already
        bool nameExists = false;
        for (int i = 0; i < playerNames.Count; i++)
        {
            if (playerNames[i].text == playerName)
            {
                nameExists = true;
            }
        }

        // If it doesn't, create new nametag based on player name
        if (!nameExists)
        {
            Vector3 playerPosition = new Vector3(player.transform.position.x, 2, player.transform.position.z);

            TMP_Text name = nameTags[playerID];
            name.GetComponent<Transform>().position = playerPosition; // Sets the nametag position over the head of the player model
            name.text = playerName; // Sets the text of the nametag
            playerNames.Add(name); // Adds TMP_text object to list for monitoring
        }

        if(!animationPlaying)
        {
            // Play random animations in playerselect
            StartCoroutine(PlayRandomAnimation());
            animationPlaying = true;
        }
    }

    /// <summary>
    /// Coroutine that continuously plays random idle animations in the player select menu
    /// </summary>
    IEnumerator PlayRandomAnimation()
    {
        yield return new WaitForSeconds(20f);
        while (true)
        {
            int randomAnimatorNumber = Random.Range(0, playerInputManagerController.playerInputs.Count); // Choose random playermodel to animate

            Animator randomAnimator = playerAnimators[randomAnimatorNumber]; // Get the animator for one of the players that has a connected input
            string randomTrigger = randomAnimator.GetParameter(Random.Range(0, randomAnimator.parameterCount)).name; // Choose a random trigger to set
            

            if ((randomTrigger == "CardPeek" || randomTrigger == "CardPeekReaction") && (playerInputManagerController.playerInputs.Count > 1) && (cardPeekCounter == 0))
            {
                if(randomAnimatorNumber == playerInputManagerController.playerInputs.Count - 1 && randomTrigger == "CardPeek")
                {
                    randomAnimatorNumber--; // Make sure Cardpeek animation cannot be played on the left player
                    randomAnimator = playerAnimators[randomAnimatorNumber]; // Set new random animator to mach newly chosen player model
                }
                else if (randomAnimatorNumber == 0 && randomTrigger == "CardPeekReaction")
                {
                    randomAnimatorNumber++; // Make sure Cardpeekreaction animation cannot be played on the right player
                    randomAnimator = playerAnimators[randomAnimatorNumber]; // Set new random animator to mach newly chosen player model
                }
                

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
