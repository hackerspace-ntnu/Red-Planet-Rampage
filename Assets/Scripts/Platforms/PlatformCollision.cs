using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlatformCollision: MonoBehaviour
{

    [SerializeField] private PlatformMovement platformMovement;
    private List<Transform> startRoutepoints;
    private void Start() {
        startRoutepoints = platformMovement.routepoints;
    }
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")){
            platformMovement.routepoints.Reverse();
        }
    }
}