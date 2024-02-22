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
    private float failDegrees = 50f;
    private const float minArrowTravelTime = 6f;
    private const float maxArrowTravelTime = 10f;
    private const float degreeStepSize = 45f;
    private const float stepSizeFirerateMultiplier = 3f;

    [Header("Sub Components")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ParticleSystem exhaustParticles;
    [SerializeField]
    private ParticleSystem failedExhaustParticles;
    [SerializeField]
    private PlayerHand playerHandLeft;
    [SerializeField]
    private PlayerHand playerHandRight;
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

    private AudioSource audioSource;
    [SerializeField]
    private AudioGroup plopSounds;

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

        mowerScreen.SetFloat("_ArrowDegrees", minArrowDegrees);
        StartArrowWobbleTween();

        if (!gunController.Player)
            return;
        audioSource = GetComponent<AudioSource>();
        playerHandLeft.gameObject.SetActive(true);
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        playerHandRight.SetPlayer(gunController.Player);
        handAnimator = GetComponent<Animator>();
        LineHoldingPoint = playerHandLeft.HoldingPoint;
        handString.gameObject.SetActive(true);

        if (gunController.Player.GunOrigin.TryGetComponent(out GunController gunControllerDisplay))
            gunControllerDisplay.GetComponentInChildren<LawnMower>().LineHoldingPoint = gunController.Player.PlayerIK.LeftHandIKTransform;
    }

    private void OnDestroy()
    {
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= DisableLine;
    }

    private void LateUpdate()
    {
        if (!handString || !lineStart || !LineHoldingPoint)
            return;
        handString.SetPosition(0, lineStart.position);
        handString.SetPosition(1, LineHoldingPoint.position);
    }

    private void Fire(GunStats stats)
    {
        LeanTween.cancel(gameObject);
        animator.SetTrigger("PistonPump");
        if (gunController)
            plopSounds.Play(audioSource);
        handAnimator.SetTrigger("Pull");
        exhaustParticles.Play();
        var currentDegrees = mowerScreen.GetFloat("_ArrowDegrees");
        if (currentDegrees < failDegrees)
        {
            success = true;
            currentDegrees += degreeStepSize - stepSizeFirerateMultiplier * stats.Firerate.Value();
            currentDegrees = Mathf.Min(currentDegrees, maxArrowDegrees);
            Reload(stats);
        }
        else
        {
            success = false;
            failedExhaustParticles.Play();
        }

        gameObject.LeanValue(
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree),
            currentDegrees, minArrowDegrees,
            MathF.Max(minArrowTravelTime, maxArrowTravelTime - stats.Firerate.Value()))
            .setOnComplete(() =>
            {
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
            (degree) => mowerScreen.SetFloat("_ArrowDegrees", degree), minArrowDegrees + 4f, minArrowDegrees - 4f, 0.15f)
            .setLoopPingPong();
    }

    private void DisableLine()
    {
        handString.gameObject.SetActive(false);
    }
}
