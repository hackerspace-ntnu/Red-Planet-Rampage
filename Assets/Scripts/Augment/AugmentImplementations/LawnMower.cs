using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LawnMower : GunBody
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private int mowerScreenMaterialIndex = 0;
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;
    [SerializeField]
    private Material mowerScreen;
    [SerializeField]
    private float minArrowDegrees;
    [SerializeField]
    private float maxArrowDegrees;
    [SerializeField]
    private float arrowHitSpan = 5f;
    [SerializeField]
    private AnimationCurve targetRateOfChange;

    private float currentTarget;
    public override void Start()
    {
        meshRenderer.materials[mowerScreenMaterialIndex] = Instantiate(meshRenderer.materials[mowerScreenMaterialIndex]);
        mowerScreen = meshRenderer.materials[mowerScreenMaterialIndex];
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("MowerBody not attached to gun parent!");
            return;
        }
        gunController.onFire += Fire;

        currentTarget = maxArrowDegrees;
        mowerScreen.SetFloat("_TargetDegrees", currentTarget);
        mowerScreen.SetFloat("_ArrowDegrees", maxArrowDegrees);
        gameObject.LeanValue(
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree), maxArrowDegrees + 4f, maxArrowDegrees - 4f, 0.15f)
            .setLoopPingPong();

    }

    private void Fire(GunStats stats)
    {
        LeanTween.cancel(gameObject);
        animator.SetTrigger("PistonPump");
        var currentDegrees = mowerScreen.GetFloat("_ArrowDegrees");
        if (currentDegrees > currentTarget - arrowHitSpan && currentDegrees < currentTarget + arrowHitSpan)
        {
            var deltaDegrees = targetRateOfChange.Evaluate(Mathf.InverseLerp(minArrowDegrees, maxArrowDegrees, currentTarget - 5f));
            currentTarget = Mathf.Lerp(minArrowDegrees, maxArrowDegrees, deltaDegrees);
            mowerScreen.SetFloat("_TargetDegrees", currentTarget);
            Reload(stats);
        }
        else
        {
            mowerScreen.SetFloat("_TargetDegrees", maxArrowDegrees);
        }
        StartTween(stats);
    }

    protected override void Reload(GunStats stats)
    {
        gunController.Reload(1f); 
    }

    private void StartTween(GunStats stats)
    {
        gameObject.LeanValue( 
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree), minArrowDegrees, maxArrowDegrees, 5f)
            .setOnComplete(() => 
            {
                currentTarget = maxArrowDegrees;
                mowerScreen.SetFloat("_TargetDegrees", currentTarget);
                Reload(stats);
                gameObject.LeanValue(
                    (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree), maxArrowDegrees + 4f, maxArrowDegrees - 4f, 0.15f)
                    .setLoopPingPong();
            });
    }
}
