using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windmill : MonoBehaviour
{
    [SerializeField]
    private GameObject motor;

    // TODO: synchronize to wind direction later
    void Start()
    {
        motor.LeanRotateAroundLocal(Vector3.forward, 15, 4f).setEaseInOutQuad().setLoopPingPong();
    }
}
