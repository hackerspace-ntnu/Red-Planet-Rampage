using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[Serializable]
public struct Precision
{
    public float range;
    public string text;
    public Color color;
    public float awardFactor;
    public AudioGroup audio;
}

public enum ArrowDirection
{
    EAST,
    NORTH,
    WEST,
    SOUTH,
}

public class DDRBody : GunBody
{
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;

    [SerializeField]
    private TMP_Text precisionText;

    [SerializeField]
    private AugmentAnimator animator;

    [SerializeField]
    private PlayerHand playerHandLeft;
    [SerializeField]
    private PlayerHand playerHandRight;

    private Material ddrMaterial;

    private const int screenMaterialIndex = 2;

    private float arrowHeight = 0;
    private ArrowDirection arrowDirection = ArrowDirection.NORTH;
    private ArrowDirection inputDirection = ArrowDirection.NORTH;

    private const float screenHeight = 4f;
    private const float errorMarginInput = 0.1f;

    private const float targetHeight = 3;
    private const float startHeight = targetHeight - screenHeight;
    private const float lowestCheckHeight = 2.5f;

    [SerializeField]
    private Precision[] precisionsGoodToBad;

    private int arrowMoverTween;
    private int screenFlasherTween;
    private int screenPulseAnimatorTween;
    private int textAnimatorTween;
    private int targetArrowScaleTween;
    private int inputArrowScaleTween;
    private int targetArrowRotationTween;
    private int inputArrowRotationTween;

    private float secondsPerUnitHeight;
    private float musicPace;

    private AudioSource audioSource;
    private InputManager inputManager;

    public override void Start()
    {
        meshRenderer.materials[screenMaterialIndex] = Instantiate(meshRenderer.materials[screenMaterialIndex]);
        ddrMaterial = meshRenderer.materials[screenMaterialIndex];
        precisionText.enabled = false;

        musicPace = 60f / MusicTrackManager.Singleton.BeatsPerMinute;
        var secondsPerArrow = MusicTrackManager.Singleton.BeatsPerBar * musicPace;
        secondsPerUnitHeight = secondsPerArrow / (targetHeight - startHeight);

        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;

        if (gunController.Player)
        {
            inputManager = gunController.Player.inputManager;
            inputManager.onFirePerformed += Fire;
            inputManager.onMovePerformed += ArrowSelect;

            var delay = (float)(MusicTrackManager.Singleton.IsfadingOutPreviousTrack
                ? MusicTrackManager.Singleton.TrackOffset
                : secondsPerArrow - (MusicTrackManager.Singleton.TimeSinceTrackStart % secondsPerArrow));

            PickNewTargetDirection();

            arrowMoverTween = LeanTween.value(gameObject, SetArrowHeigth, startHeight, screenHeight, secondsPerUnitHeight * (screenHeight - startHeight))
                .setDelay(delay)
                .setRepeat(-1).id;

            screenPulseAnimatorTween = LeanTween.value(gameObject, SetBackgroundZoom, 0.5f, 1.5f, musicPace)
                .setDelay(delay)
                .setLoopPingPong()
                .setOnComplete(
                () => animator.OnFire(gunController.stats)).id;

            animator.OnInitialize(gunController.stats);

            playerHandLeft.SetPlayer(gunController.Player);
            playerHandLeft.gameObject.SetActive(true);
            playerHandRight.SetPlayer(gunController.Player);
            playerHandRight.gameObject.SetActive(true);

            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (!inputManager)
            return;
        UpdateInputArrow();
    }

    protected override void Reload(GunStats stats)
    {
        Precision? precision = null;

        if (Mathf.Abs(arrowHeight - targetHeight) <= precisionsGoodToBad[0].range)
            precision = precisionsGoodToBad[0];
        else if (Mathf.Abs(arrowHeight - targetHeight) <= precisionsGoodToBad[1].range)
            precision = precisionsGoodToBad[1];
        else if (Mathf.Abs(arrowHeight - targetHeight) <= precisionsGoodToBad[2].range)
            precision = precisionsGoodToBad[2];
        else if (Mathf.Abs(arrowHeight - targetHeight) <= precisionsGoodToBad[3].range)
            precision = precisionsGoodToBad[3];

        if (!precision.HasValue)
            return;
        if (LeanTween.isTweening(textAnimatorTween))
            LeanTween.cancel(textAnimatorTween);
        precisionText.enabled = true;
        precisionText.text = precision.Value.text;
        precisionText.color = precision.Value.color;
        textAnimatorTween = LeanTween.scale(precisionText.gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.5f)
            .setEasePunch()
            .setOnComplete(
            () => precisionText.enabled = false).id;
        if (LeanTween.isTweening(screenFlasherTween))
            LeanTween.cancel(screenFlasherTween);
        screenFlasherTween = LeanTween.value(gameObject, SetFlashFactor, 0, 50f, 0.5f).setEasePunch().id;

        gunController.Reload(reloadEfficiencyPercentage * precision.Value.awardFactor);
        animator.OnReload(gunController.stats);

        if (precision.Value.audio)
        {
            precision.Value.audio.Play(audioSource);
        }
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        animator.OnFire(gunController.stats);
    }

    private void ArrowSelect(InputAction.CallbackContext ctx)
    {
        AnimateInputArrowScale(1.5f);

        switch (arrowDirection)
        {
            case ArrowDirection.NORTH:
                if (!(inputManager.moveInput.y > 1 - errorMarginInput))
                    return;
                break;
            case ArrowDirection.EAST:
                if (!(inputManager.moveInput.x > 1 - errorMarginInput))
                    return;
                break;
            case ArrowDirection.SOUTH:
                if (!(inputManager.moveInput.y < -1 + errorMarginInput))
                    return;
                break;
            case ArrowDirection.WEST:
                if (!(inputManager.moveInput.x < -1 + errorMarginInput))
                    return;
                break;
        }

        if (arrowHeight < lowestCheckHeight)
        {
            AnimateTargetArrowScale(1.5f);
            return;
        }

        AnimateTargetArrowScale(1.8f);

        Reload(gunController.stats);
        ResetAndStartArrowTween();
    }

    private void UpdateInputArrow()
    {
        var oldDirection = inputDirection;

        if (inputManager.moveInput.y > 1 - errorMarginInput)
            inputDirection = ArrowDirection.NORTH;
        else if (inputManager.moveInput.x > 1 - errorMarginInput)
            inputDirection = ArrowDirection.EAST;
        else if (inputManager.moveInput.y < -1 + errorMarginInput)
            inputDirection = ArrowDirection.SOUTH;
        else if (inputManager.moveInput.x < -1 + errorMarginInput)
            inputDirection = ArrowDirection.WEST;

        if (oldDirection != inputDirection)
            AnimateInputArrowRotation(oldDirection, inputDirection);
    }

    private void ResetAndStartArrowTween()
    {
        if (LeanTween.isTweening(arrowMoverTween))
            LeanTween.cancel(arrowMoverTween);
        PickNewTargetDirection();
        arrowMoverTween = LeanTween.value(gameObject, SetArrowHeigth, arrowHeight, screenHeight, secondsPerUnitHeight * (screenHeight - arrowHeight))
            .setRepeat(-1).id;
    }

    private void SetArrowHeigth(float heigth)
    {
        ddrMaterial.SetFloat("_ArrowYPos", heigth);
        arrowHeight = heigth;
    }

    private void SetFlashFactor(float factor)
    {
        ddrMaterial.SetFloat("_FlashFactor", factor);
    }

    private void SetBackgroundZoom(float zoom)
    {
        ddrMaterial.SetVector("_BackgroundZoom", new Vector4(zoom, zoom, 0, 0));
    }

    private void AnimateTargetArrowScale(float to)
    {
        if (LeanTween.isTweening(targetArrowScaleTween))
            LeanTween.cancel(targetArrowScaleTween);
        targetArrowScaleTween = LeanTween.value(gameObject, SetTargetArrowScale, 1, to - 1, .3f).setEasePunch().id;
    }

    private void AnimateInputArrowScale(float to)
    {
        if (LeanTween.isTweening(inputArrowScaleTween))
            LeanTween.cancel(inputArrowScaleTween);
        inputArrowScaleTween = LeanTween.value(gameObject, SetInputArrowScale, 1, to - 1, .3f).setEasePunch().id;
    }

    private void SetTargetArrowScale(float scale)
    {
        ddrMaterial.SetFloat("_TargetScale", scale);
    }

    private void SetInputArrowScale(float scale)
    {
        ddrMaterial.SetFloat("_InputScale", scale);
    }

    private void AnimateTargetArrowRotation(ArrowDirection from, ArrowDirection to)
    {
        targetArrowRotationTween = AnimateArrowRotation(targetArrowRotationTween, SetTargetArrowRotation, from, to);
    }

    private void AnimateInputArrowRotation(ArrowDirection from, ArrowDirection to)
    {
        inputArrowRotationTween = AnimateArrowRotation(inputArrowRotationTween, SetInputArrowRotation, from, to);
    }

    private int AnimateArrowRotation(int tween, Action<float> setter, ArrowDirection from, ArrowDirection to)
    {
        if (LeanTween.isTweening(tween))
            LeanTween.cancel(tween);

        var fromDegrees = ArrowDirectionToDegrees((int)from);
        var toDegrees = ArrowDirectionToDegrees((int)to);

        // Ensure reasonable transition when wrapping around
        if (from == ArrowDirection.EAST && to == ArrowDirection.SOUTH)
        {
            fromDegrees = 360;
        }
        else if (from == ArrowDirection.SOUTH && to == ArrowDirection.EAST)
        {
            fromDegrees = -90;
        }

        return LeanTween.value(gameObject, setter, fromDegrees, toDegrees, .2f).id;
    }

    private void SetTargetArrowRotation(float degrees)
    {
        ddrMaterial.SetFloat("_TargetRotationDegrees", degrees);
    }

    private void SetInputArrowRotation(float degrees)
    {
        ddrMaterial.SetFloat("_InputRotationDegrees", degrees);
    }

    private float ArrowDirectionToDegrees(int direction)
    {
        // Default orientation (0 degrees) = 3 O' clock
        return direction * 90;
    }

    private void PickNewTargetDirection()
    {
        // Should be 0 when the arrow passes the top of the screen
        // and -0.5 when the arrow hits the mark
        // and less than -0.5 when the arrow was too early
        arrowHeight = arrowHeight - targetHeight - Mathf.Abs(startHeight);
        var oldDirection = arrowDirection;
        arrowDirection = (ArrowDirection)Random.Range(0, 4);
        AnimateTargetArrowRotation(oldDirection, arrowDirection);
    }

    private void OnDestroy()
    {
        if (!inputManager)
            return;

        inputManager.onFirePerformed -= Fire;
        inputManager.onMovePerformed -= ArrowSelect;
    }
}
