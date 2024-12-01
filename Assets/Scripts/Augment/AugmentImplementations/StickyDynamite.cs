using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class StickyDynamite : StuckObject
{
    [SerializeField]
    private VisualEffect smoke;
    [SerializeField]
    private Renderer mesh;
    [SerializeField]
    private Renderer explosiveBarrel;
    [SerializeField]
    private VisualEffect[] barrelSmoke;
    [SerializeField]
    private ExplosionController explosion;
    public ExplosionController Explosion => explosion;

    private void OnEnable()
    {
        ResetDynamite();
    }

    public void ResetDynamite()
    {
        explosion.StopAllCoroutines();
        mesh.enabled = true;
        smoke.enabled = true;
        if (explosion.transform.parent)
            explosion.transform.parent.gameObject.SetActive(true);
        explosion.gameObject.SetActive(true);
        explosion.Init();
    }

    public void SetBarrel()
    {
        mesh.enabled = false;
        explosiveBarrel.enabled = true;
        smoke.enabled = false;
        barrelSmoke.ToList().ForEach(smoke => smoke.enabled = true);
    }

    public void Detonate(PlayerManager sourcePlayer)
    {
        mesh.enabled = false;
        explosiveBarrel.enabled = false;
        smoke.enabled = false;
        barrelSmoke.ToList().ForEach(smoke => smoke.enabled = false);
        // Double check that dynamite hasn't been taken by the pool before exploding
        if (explosion.gameObject.activeSelf)
        {
            explosion.Explode(sourcePlayer);
            explosion.transform.parent.parent = null;
        }
    }
}
