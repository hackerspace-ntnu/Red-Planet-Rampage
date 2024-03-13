using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLock : MonoBehaviour
{
    [SerializeField]
    private Vector3 initialRotation;
    private Quaternion rotation;
    void Start()
    {
        rotation = Quaternion.Euler(initialRotation);
    }

    void Update()
    {
        transform.rotation = rotation;
    }
}
