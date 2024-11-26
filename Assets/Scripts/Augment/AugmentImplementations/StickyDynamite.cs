using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class StickyDynamite : StuckObject
{
    [SerializeField]
    private VisualEffect smoke;
    [SerializeField]
    private Renderer mesh;
    [SerializeField]
    private ExplosionController explosion;
    public void Detonate(PlayerManager sourcePlayer)
    {
        mesh.enabled = false;
        smoke.enabled = false;
        explosion.Explode(sourcePlayer);
        explosion.transform.parent.parent = null;
    }
}
