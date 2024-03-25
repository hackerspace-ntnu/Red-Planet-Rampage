using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private List<Vector3> collisionPoints = new List<Vector3>();
    [SerializeField]
    private Transform anchor;
    [SerializeField]
    public Transform Target { get; set; }
    [SerializeField]
    private LayerMask colliderLayers;
    [SerializeField]
    private LineRenderer line;

    void Start()
    {
        if (!anchor)
        {
            anchor = new GameObject().transform;
            anchor.position = transform.position;
        }
        collisionPoints.Add(anchor.position);
    }

    private void FixedUpdate()
    {
        if (!anchor || !Target)
            return;

        if (Physics.Linecast(Target.position, collisionPoints[collisionPoints.Count - 1], out RaycastHit hitInfo, colliderLayers))
        {
            if (collisionPoints.Count > 1)
            {
                if (Vector3.Distance(hitInfo.point, collisionPoints[collisionPoints.Count - 1]) > 0.2f)
                    collisionPoints.Add(hitInfo.point);
            }
            // First point, nothing to compare to
            else
                collisionPoints.Add(hitInfo.point);
        }
        if (collisionPoints.Count > 1)
        {
            // Extra failsafe in case rope gets stuck on corners
            var previousAnchor = collisionPoints[collisionPoints.Count - 2];
            var activeAnchor = collisionPoints[collisionPoints.Count - 1];
            if (Vector3.Dot((activeAnchor - previousAnchor).normalized, (activeAnchor - Target.position).normalized) > 0.8f)
                collisionPoints.RemoveAt(collisionPoints.Count - 1);

            if (!Physics.Linecast(Target.position, previousAnchor, out RaycastHit previousHit, colliderLayers) || previousHit.distance < 0.2f)
                collisionPoints.RemoveAt(collisionPoints.Count - 1);
        }
        // +1 to add target vertex at the end
        line.positionCount = collisionPoints.Count + 1;
        line.SetPositions(collisionPoints.ToArray());
        line.SetPosition(collisionPoints.Count, Target.position);
    }
}
