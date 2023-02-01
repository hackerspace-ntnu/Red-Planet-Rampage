using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunExtension : GunModifier
{

    public override int priority { get => 2; }

    [SerializeField]
    private GameObject model;

    // The projectile is shot from each of these outputs, might want to change this iw we want varying outputs, like alternate shooting
    [SerializeField]
    public Transform[] outputs;

    /// <summary>
    /// Attaches the extension model(s) to each of the attachment points of the barrel.
    /// Required since a barrel can have multiple outputs (minigun).
    /// Note that the extension should be instantiated once before this method is called.
    /// </summary>
    /// <param name="transforms">The barrel's attachment points</param>
    /// <returns>Combined output transforms from barrel and extension</returns>
    public Transform[] AttachToTransforms(Transform[] transforms)
    {
        var attachedOutputs = new List<Transform>();
        foreach (var t in transforms[1..])
        {
            Instantiate(model, t.position, t.rotation, transform);
            foreach (var output in outputs)
            {
                // TODO Do we need a rotational offset?
                var offset = output.position - transforms[0].position;
                var outputInstance = Instantiate(output, t.position + offset, t.rotation, transform);
                attachedOutputs.Add(outputInstance.transform);
            }
        }
        return attachedOutputs.ToArray();
    }
}
