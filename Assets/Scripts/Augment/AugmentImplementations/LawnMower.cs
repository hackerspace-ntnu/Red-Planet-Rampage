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
    [SerializeField]
    private ParticleSystem exhaustParticles;
    [SerializeField]
    private ParticleSystem failedExhaustParticles;
    [SerializeField]
    private PlayerHand playerHand;
    [SerializeField]
    private LineRenderer handString;
    private Animator handAnimator;
    [SerializeField]
    private Transform lineStart;

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

        if (!gunController.player)
            return;

        playerHand.gameObject.SetActive(true);
        playerHand.SetPlayer(gunController.player);
        handAnimator = GetComponent<Animator>();
        handString.gameObject.SetActive(true);
    }

    private void Update()
    {
        handString.SetPosition(0, lineStart.position);
        handString.SetPosition(1, playerHand.HoldingPoint.position);
    }

    private void Fire(GunStats stats)
    {
        LeanTween.cancel(gameObject);
        animator.SetTrigger("PistonPump");
        handAnimator.SetTrigger("Pull");
        exhaustParticles.Play();
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
            failedExhaustParticles.Play();
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
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree), minArrowDegrees, maxArrowDegrees, 3f)
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
