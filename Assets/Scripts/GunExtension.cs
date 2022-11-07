using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunExtension : GunModifyer
{

    public override int priority { get => 2; }

    [SerializeField]
    private GameObject model;

    [SerializeField]
    public Transform[] outputs;

    public void AttachToTransforms(Transform[] transforms)
    {
        foreach(var t in transforms[1..])
        {
            Instantiate(model, t.position, t.rotation, transform);
        }
    }
}
