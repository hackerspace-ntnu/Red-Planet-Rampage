using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSunMovement : MonoBehaviour
{
    [SerializeField]
    private float RotationDegrees = 360;
    [SerializeField]
    private float RotationSeconds = 360;

    void Start()
    {
        transform.LeanRotateAroundLocal(Vector3.up, RotationDegrees, RotationSeconds).setLoopCount(-1);
    }
}
