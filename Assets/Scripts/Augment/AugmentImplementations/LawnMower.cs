using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LawnMower : GunBody
{
    [Header("Functional Parameters")]
    [SerializeField]
    private float minArrowDegrees;
    [SerializeField]
    private float maxArrowDegrees;
    [SerializeField]
    private float arrowHitSpan = 5f;
    [SerializeField]
    private AnimationCurve targetRateOfChange;
    private const float minArrowTravelTime = 4f;
    private const float maxArrowTravelTime = 6f;
    private const float degreeStepSize = 5f;
    private float currentTarget;

    [Header("Sub Components")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ParticleSystem exhaustParticles;
    [SerializeField]
    private ParticleSystem failedExhaustParticles;
    [SerializeField]
    private PlayerHand playerHand;
    [SerializeField]
    private Transform lineStart;
    [SerializeField]
    private LineRenderer handString;
    private Animator handAnimator;

    [Header("Material parameters")]
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;
    [SerializeField]
    private int mowerScreenMaterialIndex = 0;
    [SerializeField]
    private Material mowerScreen;

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
        StartArrowWobbleTween();

        if (!gunController.player)
            return;

        playerHand.gameObject.SetActive(true);
        playerHand.SetPlayer(gunController.player);
        handAnimator = GetComponent<Animator>();
        handString.gameObject.SetActive(true);
        if (!MatchController.Singleton)
            return;
        MatchController.Singleton.onRoundEnd += DisableLine; 
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
        var isInLowerTreshold = currentDegrees < currentTarget + arrowHitSpan;
        var isInHigherTreshold = currentDegrees > currentTarget - arrowHitSpan;
        if (isInLowerTreshold && isInHigherTreshold)
        {
            var deltaDegrees = targetRateOfChange.Evaluate(Mathf.InverseLerp(minArrowDegrees, maxArrowDegrees, currentTarget - degreeStepSize));
            currentTarget = Mathf.Lerp(minArrowDegrees, maxArrowDegrees, deltaDegrees);
            mowerScreen.SetFloat("_TargetDegrees", currentTarget);
            Reload(stats);
        }
        else
        {
            mowerScreen.SetFloat("_TargetDegrees", maxArrowDegrees);
            failedExhaustParticles.Play();
        }

        gameObject.LeanValue(
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree),
            minArrowDegrees, maxArrowDegrees,
            MathF.Max(minArrowTravelTime, maxArrowTravelTime - stats.Firerate.Value()))
            .setOnComplete(() =>
            {
                currentTarget = maxArrowDegrees;
                mowerScreen.SetFloat("_TargetDegrees", currentTarget);
                Reload(stats);
                StartArrowWobbleTween();
            });
    }

    protected override void Reload(GunStats stats)
    {
        gunController.Reload(1f); 
    }

    private void StartArrowWobbleTween()
    {
        gameObject.LeanValue(
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree), maxArrowDegrees + 4f, maxArrowDegrees - 4f, 0.15f)
            .setLoopPingPong();
    }

    private void DisableLine()
    {
        handString.gameObject.SetActive(false);
    }
}