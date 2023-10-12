using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
[SerializeField] private List<Transform> routepoints;
[SerializeField] private float moveSpeed = 5f;
private int nextRoutepointIndex;
private float travelDistance;
private void Start()
{
    nextRoutepointIndex = 0;

    travelDistance = Vector3.Distance(routepoints[nextRoutepointIndex + 1].transform.position, transform.position);
    if (routepoints.Count <= 0){
        Debug.LogWarning("No routepoints specified");
        return;
    } 
}

private void FixedUpdate() {
    
    MovePlatform();
}
private void MovePlatform()
{
    float currentDistance = Vector3.Distance(routepoints[nextRoutepointIndex].transform.position, transform.position);

    transform.position = Vector3.MoveTowards(transform.position, routepoints[nextRoutepointIndex].transform.position,
    (moveSpeed * Time.deltaTime));

    if (currentDistance <= 0)
    {
        nextRoutepointIndex++;
    }
     if (currentDistance < 2 || currentDistance + 2 >= travelDistance)
    {
        moveSpeed = 2f;

        Debug.Log("travel:" + travelDistance);
        Debug.Log("current:" + currentDistance);

    }else{
        moveSpeed = 5f;
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