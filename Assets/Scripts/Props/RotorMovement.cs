using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotorMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject rotor;
    void Start()
    {
        if (rotor)
            rotor.LeanRotateAroundLocal(Vector3.forward, 360, 1).setLoopClamp();
    }
}
