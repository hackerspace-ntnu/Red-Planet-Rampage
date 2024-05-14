using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalRingSpinner : MonoBehaviour
{
    public Transform[] rotationTransforms = null;

    public Vector3[] rotationAxes = null;

    public float[] rotationSpeed = null;

    void Update()
    {
        for(int i = 0; i < rotationTransforms.Length; i++)
            rotationTransforms[i].localRotation = Quaternion.AngleAxis(rotationSpeed[i]*Time.deltaTime, rotationAxes[i]) * rotationTransforms[i].localRotation;
    }
}
