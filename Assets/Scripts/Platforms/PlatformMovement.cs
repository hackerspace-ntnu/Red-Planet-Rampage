using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlatformMovement : NetworkBehaviour
{
    public List<Transform> routepoints;

    [SerializeField]
    private float forcePerSpeed = 10;

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

    private Vector3 currentDirection;
    private float currentSpeed;

    private void Start()
    {
        nextRoutepointIndex = 0;

        travelDistance = Vector3.Distance(routepoints[nextRoutepointIndex + 1].transform.position, transform.position);

        if (rotor)
            rotor.LeanRotateAroundLocal(Vector3.forward, 360, 1).setLoopClamp();

        if (routepoints.Count <= 0)
        {
            Debug.LogWarning("No routepoints specified");
            return;
        }
    }

    private void FixedUpdate()
    {
        if (!isNetworked || isServer)
            MovePlatform();
    }

    private void MovePlatform()
    {
        float currentDistance = Vector3.Distance(routepoints[nextRoutepointIndex].transform.position, transform.position);
        currentDirection = (routepoints[nextRoutepointIndex].transform.position - transform.position).normalized;

        // Smoothly decelerate/accelerate at endpoints
        var distanceFromClosesEndpoint = Mathf.Min(currentDistance, Mathf.Abs(travelDistance - currentDistance));
        currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, distanceFromClosesEndpoint / accelerationDistance);

        transform.position = Vector3.MoveTowards(transform.position, routepoints[nextRoutepointIndex].transform.position,
            currentSpeed * Time.deltaTime);

        if (currentDistance <= 0)
        {
            nextRoutepointIndex++;
        }

        if (nextRoutepointIndex != routepoints.Count) return;

        routepoints.Reverse();
        nextRoutepointIndex = 0;
    }


    private void OnTriggerStay(Collider other)
    {
        // Only local human players should get the effect...
        if (!other.gameObject.TryGetComponent(out PlayerManager playerManager) || !playerManager.inputManager)
            return;

        if (!other.gameObject.TryGetComponent<Rigidbody>(out var rigidbody))
            return;

        var force = currentSpeed * forcePerSpeed * currentDirection;
        rigidbody.AddForce(force, ForceMode.Acceleration);
    }
}
