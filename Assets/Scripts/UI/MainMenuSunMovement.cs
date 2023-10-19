using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSunMovement : MonoBehaviour
{
    public float RotationDegrees = 360;
    public float RotationSeconds = 360;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.LeanRotateAroundLocal(Vector3.up, RotationDegrees, RotationSeconds).setLoopCount(-1);
    }
}
