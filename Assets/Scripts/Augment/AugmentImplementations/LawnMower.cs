using System;
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
    public Transform LineHoldingPoint;

    [Header("Material parameters")]
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;
    [SerializeField]
    private int mowerScreenMaterialIndex = 0;
    [SerializeField]
    private Material mowerScreen;

    private bool success = false;

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
        gunController.onFireStart += Fire;
        gunController.onFireEnd += FireEnd;

        currentTarget = maxArrowDegrees;
        mowerScreen.SetFloat("_TargetDegrees", currentTarget);
        mowerScreen.SetFloat("_ArrowDegrees", maxArrowDegrees);
        StartArrowWobbleTween();

        if (!gunController.Player)
            return;

        playerHand.gameObject.SetActive(true);
        playerHand.SetPlayer(gunController.Player);
        handAnimator = GetComponent<Animator>();
        LineHoldingPoint = playerHand.HoldingPoint;
        handString.gameObject.SetActive(true);

        if (!MatchController.Singleton)
            return;
        MatchController.Singleton.onRoundEnd += DisableLine;

        if (gunController.Player.GunOrigin.TryGetComponent(out GunController gunControllerDisplay))
            gunControllerDisplay.GetComponentInChildren<LawnMower>().LineHoldingPoint = gunController.Player.PlayerIK.LeftHandIKTransform;
    }

    private void LateUpdate()
    {
        handString.SetPosition(0, lineStart.position);
        handString.SetPosition(1, LineHoldingPoint.position);
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
            success = true;
            var deltaDegrees = targetRateOfChange.Evaluate(Mathf.InverseLerp(minArrowDegrees, maxArrowDegrees, currentTarget - degreeStepSize));
            currentTarget = Mathf.Lerp(minArrowDegrees, maxArrowDegrees, deltaDegrees);
            mowerScreen.SetFloat("_TargetDegrees", currentTarget);
            Reload(stats);
        }
        else
        {
            success = false;
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

    private void FireEnd(GunStats stats)
    {
        if (success) Reload(stats);
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
