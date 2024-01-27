using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSelectManager : MonoBehaviour
{
    [SerializeField]
    // Player 3d models in player select screen
    private List<GameObject> playerModels;
    [SerializeField]
    private Camera playerSelectCam;
    [SerializeField]
    private List<TMP_Text> nameTags;

    private PlayerInputManagerController playerInputManagerController;
    private List<TMP_Text> playerNames;

    public void Start()
    {
        playerInputManagerController = PlayerInputManagerController.Singleton;
        playerNames = new List<TMP_Text>();
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
    }
}
