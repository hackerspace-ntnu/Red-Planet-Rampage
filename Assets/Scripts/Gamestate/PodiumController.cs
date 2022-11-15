using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PodiumController : MonoBehaviour
{
    private List<FPSInputManager> playersInRadius;

    private MatchController matchController = MatchController.Singleton;

    void Start()
    {
    
    }

    private void OnTriggerEnter(Collider other)
    {
        FPSInputManager player = GetComponentInChildren<FPSInputManager>();
        if (player == null) { return; }
        player.onFire += AddBid;
        playersInRadius.Add(player);
    }

    private void OnTriggerExit(Collider other)
    {
        FPSInputManager player = GetComponentInChildren<FPSInputManager>();
        if (player == null) { return; }
        player.onFire -= AddBid;
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
