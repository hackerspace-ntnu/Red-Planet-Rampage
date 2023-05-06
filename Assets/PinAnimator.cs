using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private float maxDist;

    private float time;

    [SerializeField]
    private float delay = 0f;

    [SerializeField]
    private AnimationCurve easeCurve;
    void Start()
    {
        var projectileController = GetComponentInParent<ProjectileController>();

        projectileController.OnProjectileInit += (ref ProjectileState state, GunStats stats) => recoil();
        time = 1 / projectileController.stats.Firerate.Value();
    }

    private void recoil()
    {
        transform.localPosition = Vector3.zero;
        LeanTween.moveLocalZ(gameObject, maxDist, time * (1 - delay))
            .setDelay(delay * time)
            .setEase(easeCurve);
    }
}
