using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public List<Vector3> CollisionPoints = new List<Vector3>();
    [SerializeField]
    private Transform anchor;
    public Transform Anchor 
    {
        get
        {
            return anchor;
        }
        set
        {
            CollisionPoints.Add(value.position);
            anchor = value;
        }
    }
    [SerializeField]
    public Transform Target { get; set; }
    [SerializeField]
    private LayerMask colliderLayers;
    [SerializeField]
    private LineRenderer line;
    public LineRenderer Line => line;
    private float accumulatedAnchorLength = 0f;
    public bool CollisionCheckActive = true;
    public float RopeLength 
    {
        get
        {
            if (Target == null || CollisionPoints.Count < 1)
                return accumulatedAnchorLength;
            return accumulatedAnchorLength + Vector3.Distance(Target.position, CollisionPoints[CollisionPoints.Count - 1]);
        }
    }
    public Vector3 CurrentAnchor
    {
        get
        {
            return CollisionPoints[CollisionPoints.Count - 1];
        }
    }

    void Start()
    {
        if (!anchor)
            return;
        CollisionPoints.Add(anchor.position);
    }

    public void ResetRope(Vector3 position)
    {
        CollisionPoints = new List<Vector3>();
        CollisionPoints.Add(position);
        accumulatedAnchorLength = 0f;
    }
    public void ResetRope(Transform anchor)
    {
        CollisionPoints = new List<Vector3>();
        this.anchor = anchor;
        CollisionPoints.Add(anchor.position);
        accumulatedAnchorLength = 0f;
    }

    private float CollisionLength()
    {
        var accumulatedLength = 0f;
        if (CollisionPoints.Count > 1)
            for (int i = 0; i < CollisionPoints.Count - 1; i++)
                accumulatedLength += Vector3.Distance(CollisionPoints[i], CollisionPoints[i + 1]);
        return accumulatedLength;
    }

    private void FixedUpdate()
    {
        if (!anchor || !Target || CollisionPoints.Count == 0)
            return;

        if (!CollisionCheckActive)
        {
            line.positionCount = CollisionPoints.Count + 1;
            line.SetPositions(CollisionPoints.ToArray());
            line.SetPosition(CollisionPoints.Count, Target.position);
            return;
        }

        if (Physics.Linecast(Target.position, CollisionPoints[CollisionPoints.Count - 1], out RaycastHit hitInfo, colliderLayers))
        {
            if (CollisionPoints.Count > 1)
            {
                if (Vector3.Distance(hitInfo.point, CollisionPoints[CollisionPoints.Count - 1]) > 0.2f)
                {
                    CollisionPoints.Add(hitInfo.point);
                    accumulatedAnchorLength = CollisionLength();
                }
                    
            }
            // First point, nothing to compare to
            else
            {
                CollisionPoints.Add(hitInfo.point);
                accumulatedAnchorLength = CollisionLength();
            }
                
        }
        if (CollisionPoints.Count > 1)
        {
            // Extra failsafe in case rope gets stuck on corners
            var previousAnchor = CollisionPoints[CollisionPoints.Count - 2];
            var activeAnchor = CollisionPoints[CollisionPoints.Count - 1];      
            if (Vector3.Dot((activeAnchor - previousAnchor).normalized, (activeAnchor - Target.position).normalized) > 0.1f)
            {
                CollisionPoints.RemoveAt(CollisionPoints.Count - 1);
                accumulatedAnchorLength = CollisionLength();
            }
            if (!Physics.Linecast(Target.position, previousAnchor, colliderLayers) && Vector3.Distance(Target.position, previousAnchor) < Vector3.Distance(Target.position, activeAnchor) )
            {
                CollisionPoints.RemoveAt(CollisionPoints.Count - 1);
                accumulatedAnchorLength = CollisionLength();
            }
        }
        // +1 to add target vertex at the end
        line.positionCount = CollisionPoints.Count + 1;
        line.SetPositions(CollisionPoints.ToArray());
        line.SetPosition(CollisionPoints.Count, Target.position);
    }

    private void OnEnable()
    {
        line.enabled = true;
    }

    private void OnDisable()
    {
        line.enabled = false;
    }
}
