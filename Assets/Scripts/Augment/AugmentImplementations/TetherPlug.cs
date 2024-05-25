using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TetherPlug : MonoBehaviour
{
    [SerializeField]
    private Transform plugOutput;
    public HealthController Health;
    public Transform WireOrigin => plugOutput;

}
