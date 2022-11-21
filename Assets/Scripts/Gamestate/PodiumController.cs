using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PodiumController : MonoBehaviour
{
    [SerializeField]
    private AugmentType augmentType;

    private List<PlayerManager> playersInRadius;

    private MatchController matchController = MatchController.Singleton;

    void Start()
    {
    
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerManager player = GetComponentInChildren<PlayerManager>();
        if (player == null) { return; }
        player.fpsInput.onFirePerformed += AddBid;
        playersInRadius.Add(player);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerManager player = GetComponentInChildren<PlayerManager>();
        if (player == null) { return; }
        player.fpsInput.onFirePerformed -= AddBid;
        playersInRadius.Remove(player);
    }

    void AddBid(InputAction.CallbackContext ctx)
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
