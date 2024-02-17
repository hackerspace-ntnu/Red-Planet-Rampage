using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public List<Transform> routepoints;

    [SerializeField]
    private float maxSpeed = 5;

    [SerializeField]
    private float minSpeed = 2;

    [SerializeField]
    private float accelerationDistance = 2;

    [SerializeField]
    private GameObject rotor;

    private int nextRoutepointIndex;
    private float travelDistance;

    private void Start()
    {
        nextRoutepointIndex = 0;

        travelDistance = Vector3.Distance(routepoints[nextRoutepointIndex + 1].transform.position, transform.position);

        rotor.LeanRotateAroundLocal(Vector3.forward, 360, 1).setLoopClamp();

        if (routepoints.Count <= 0)
        {
            Debug.LogWarning("No routepoints specified");
            return;
        }
    }

    private void FixedUpdate()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        float currentDistance = Vector3.Distance(routepoints[nextRoutepointIndex].transform.position, transform.position);

        // Smoothly decelerate/accelerate at endpoints
        var distanceFromClosesEndpoint = Mathf.Min(currentDistance, Mathf.Abs(travelDistance - currentDistance));
        var moveSpeed = Mathf.Lerp(minSpeed, maxSpeed, distanceFromClosesEndpoint / accelerationDistance);

        transform.position = Vector3.MoveTowards(transform.position, routepoints[nextRoutepointIndex].transform.position,
            moveSpeed * Time.deltaTime);

        if (currentDistance <= 0)
        {
            nextRoutepointIndex++;
        }

        if (nextRoutepointIndex != routepoints.Count) return;

        routepoints.Reverse();
        nextRoutepointIndex = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerManager>(out PlayerManager playerManager))
        {
            other.transform.SetParent(transform);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<PlayerManager>(out PlayerManager playerManager))
        {
            other.transform.SetParent(null);
        }
    }
}
