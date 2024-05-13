using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcosphereEffectController : MonoBehaviour
{
    [SerializeField]
    private Material material;

    [SerializeField]
    private Transform orientation;

    Quaternion rotationPerS = Quaternion.Euler(90, 30, 15);
    // Update is called once per frame
    void Update()
    {
        material.SetVector("_Dir", orientation.right);
        //transform.rotation *= Quaternion.Lerp(Quaternion.identity, rotationPerS, Time.deltaTime*3);   
    }
}
