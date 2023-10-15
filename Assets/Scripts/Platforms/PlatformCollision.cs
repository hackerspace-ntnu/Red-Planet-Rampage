using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlatformCollision: MonoBehaviour
{
    [SerializeField] 
    private PlatformMovement platformMovement;

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.TryGetComponent<PlayerManager>(out PlayerManager playerManager)){
            platformMovement.routepoints.Reverse();
        }
    }
}
