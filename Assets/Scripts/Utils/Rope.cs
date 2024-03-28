using System.Collections;
using System.Collections.Generic;
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
    private float accumulatedAnchorLength = 0f;
    public float RopeLength 
    {
        get
        {
            return accumulatedAnchorLength + Vector3.Distance(Target.position, collisionPoints[collisionPoints.Count - 1]);
        }
    }
    public Vector3 CurrentAnchor
    {
        get
        {
            return collisionPoints[collisionPoints.Count - 1];
        }
    }

    void Start()
    {
        if (!anchor)
        {
            anchor = new GameObject().transform;
            anchor.position = transform.position;
        }
        collisionPoints.Add(anchor.position);
    }

    public void ResetRope(Vector3 position)
    {
        collisionPoints = new List<Vector3>();
        anchor.position = position;
        collisionPoints.Add(anchor.position);
        accumulatedAnchorLength = 0f;
    }

    private float CollisionLength()
    {
        var accumulatedLength = 0f;
        if (collisionPoints.Count > 1)
            for (int i = 0; i < collisionPoints.Count - 1; i++)
                accumulatedLength += Vector3.Distance(collisionPoints[i], collisionPoints[i + 1]);
        return accumulatedLength;
    }

    private void FixedUpdate()
    {
        if (!anchor || !Target || collisionPoints.Count == 0)
            return;

        if (Physics.Linecast(Target.position, collisionPoints[collisionPoints.Count - 1], out RaycastHit hitInfo, colliderLayers))
        {
            if (collisionPoints.Count > 1)
            {
                if (Vector3.Distance(hitInfo.point, collisionPoints[collisionPoints.Count - 1]) > 0.2f)
                {
                    collisionPoints.Add(hitInfo.point);
                    accumulatedAnchorLength = CollisionLength();
                }
                    
            }
            // First point, nothing to compare to
            else
            {
                collisionPoints.Add(hitInfo.point);
                accumulatedAnchorLength = CollisionLength();
            }
                
        }
        if (collisionPoints.Count > 1)
        {
            // Extra failsafe in case rope gets stuck on corners
            var previousAnchor = collisionPoints[collisionPoints.Count - 2];
            var activeAnchor = collisionPoints[collisionPoints.Count - 1];      
            if (Vector3.Dot((activeAnchor - previousAnchor).normalized, (activeAnchor - Target.position).normalized) > 0.1f)
            {
                collisionPoints.RemoveAt(collisionPoints.Count - 1);
                accumulatedAnchorLength = CollisionLength();
            }
            if (!Physics.Linecast(Target.position, previousAnchor, colliderLayers) && Vector3.Distance(Target.position, previousAnchor) < Vector3.Distance(Target.position, activeAnchor) )
            {
                collisionPoints.RemoveAt(collisionPoints.Count - 1);
                accumulatedAnchorLength = CollisionLength();
            }
        }
        // +1 to add target vertex at the end
        line.positionCount = collisionPoints.Count + 1;
        line.SetPositions(collisionPoints.ToArray());
        line.SetPosition(collisionPoints.Count, Target.position);
    }
}
