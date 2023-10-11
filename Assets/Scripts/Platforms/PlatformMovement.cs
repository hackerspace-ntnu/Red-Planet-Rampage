using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
[SerializeField] private List<Transform> routepoints;
[SerializeField] private float moveSpeed = 5f;
private int nextRoutepointIndex;

private void Start()
{
    if (routepoints.Count <= 0){
        Debug.LogError("No waypoints specified");
        return;
    } 
    nextRoutepointIndex = 0;
}

private void FixedUpdate() {
    
    MovePlatform();
}
private void MovePlatform()
{
  
    transform.position = Vector3.MoveTowards(transform.position, routepoints[nextRoutepointIndex].transform.position,
    (moveSpeed * Time.deltaTime));

    if (Vector3.Distance(routepoints[nextRoutepointIndex].transform.position, transform.position) <= 0)
    {
        nextRoutepointIndex++;
    }

    if (nextRoutepointIndex != routepoints.Count) return;
        routepoints.Reverse();
        nextRoutepointIndex = 0;
    }
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")){
            other.transform.SetParent(transform);
        }
    }
    private void OnTriggerExit(Collider other) {
        if(other.CompareTag("Player")){
            other.transform.SetParent(null);
        }
    }
}