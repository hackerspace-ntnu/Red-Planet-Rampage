using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleBone : MonoBehaviour
{
    [SerializeField]
    float offsetMagnitude = 0.1f;

    Vector3 lastPos = Vector3.zero;
    Vector3 lastLastPos = Vector3.zero;

    [SerializeField]
    float velocityDamping = 0.1f;
    [SerializeField]
    float springStrength = 0.1f;

    [SerializeField]
    Transform meassurementPoint = null;

    [SerializeField]
    Transform offsetTarget = null;

    public Rigidbody body;

    private float lastPlayerDiff = 0f;

    private void FixedUpdate()
    {
        Vector3 offsetVelocity = (lastPos - lastLastPos) / Time.fixedDeltaTime;

        offsetVelocity -= offsetVelocity * velocityDamping * Time.fixedDeltaTime;

        offsetVelocity += (meassurementPoint.position - lastPos) * springStrength * Time.fixedDeltaTime;

        lastLastPos = lastPos;

        lastPos += offsetVelocity * Time.fixedDeltaTime;
        if ((lastPos - meassurementPoint.position).magnitude > offsetMagnitude)
        {
            Vector3 diff = lastPos - meassurementPoint.position;
            float playerDiff = 0f;
            if (body)
                playerDiff = Mathf.Abs(lastPlayerDiff - body.velocity.magnitude);
            lastPos = meassurementPoint.position + diff.normalized * offsetMagnitude;
            lastPlayerDiff = playerDiff;
        }
        offsetTarget.position = lastPos;
    }
}
