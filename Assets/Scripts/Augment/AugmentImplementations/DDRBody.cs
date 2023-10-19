using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct Precision
{
    public float range;
    public string text;
    public Color color;
    public float awardFactor;
}
public enum ArrowDirection
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}
public class DDRBody : GunBody
{
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;

    [SerializeField]
    private TMP_Text precisionText;

    [SerializeField]
    private AugmentAnimator animator;

    private Material ddrMaterial;

    private const int screenMaterialIndex = 2;

    private float arrowHeight = 0;
    private ArrowDirection arrowDirection = ArrowDirection.NORTH;

    private const float screenHeight = 3.5f;
    private const float errorMarginInput = 0.1f;

    private const float targetHeight = 3;
    private const float startHeight = targetHeight - screenHeight;
    private const float lowestCheckHeight = 2.5f;

    [SerializeField]
    private Precision[] precisionsGoodToBad;

    LTDescr arrowMover;

    private float secondsPerUnitHeight;
    private float musicPace;


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
        {
            Debug.Log("DanceBody not attached to gun parent!");
            return;
        }

        if (gunController.player)
        {
            gunController.player.inputManager.onFirePerformed += Fire;
            gunController.player.inputManager.onMovePerformed += ArrowSelect;

            arrowMover = LeanTween.value(gameObject, SetArrowHeigth, startHeight, screenHeight, secondsPerUnitHeight * (screenHeight - startHeight))
                .setDelay(MusicTrackManager.Singleton.TrackOffset)
                .setRepeat(-1)
                .setOnComplete(ResetArrow);

            LeanTween.value(gameObject, SetBackgroundZoom, 0.5f, 1.5f, musicPace)
                .setDelay(MusicTrackManager.Singleton.TrackOffset)
                .setLoopPingPong()
                .setOnComplete(
                () => animator.OnFire(0));

            animator.OnInitialize(gunController.stats);

        }

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

        precisionText.enabled = true;
        precisionText.text = precision.Value.text;
        precisionText.color = precision.Value.color;
        precisionText.gameObject.LeanScale(new Vector3(1.1f, 1.1f, 1.1f), 0.5f)
            .setEasePunch()
            .setOnComplete(
            () => precisionText.enabled = false);

        LeanTween.value(gameObject, SetFlashFactor, 0, 20f, 0.5f).setEasePunch();

        gunController.Reload(reloadEfficiencyPercentage * precision.Value.awardFactor);
        animator.OnReload(1);
    }

    private void Fire(InputAction.CallbackContext ctx)
    {
        animator.OnFire(gunController.stats.Ammo);
    }

    private void ArrowSelect(InputAction.CallbackContext ctx)
    {
        if (arrowHeight < lowestCheckHeight)
            return;

        switch (arrowDirection)
        {
            case ArrowDirection.NORTH:
                if (!(gunController.player.inputManager.moveInput.y > 1 - errorMarginInput))
                    return;
                break;
            case ArrowDirection.EAST:
                if (!(gunController.player.inputManager.moveInput.x > 1 - errorMarginInput))
                    return;
                break;
            case ArrowDirection.SOUTH:
                if (!(gunController.player.inputManager.moveInput.y < -1 + errorMarginInput))
                    return;
                break;
            case ArrowDirection.WEST:
                if (!(gunController.player.inputManager.moveInput.x < -1 + errorMarginInput))
                    return;
                break;
        }

        Reload(gunController.stats);
        ResetAndStartArrowTween();
    }

    private void ResetAndStartArrowTween()
    {
        LeanTween.cancel(arrowMover.id);
        ResetArrow();
        arrowMover = LeanTween.value(gameObject, SetArrowHeigth, arrowHeight, screenHeight, secondsPerUnitHeight * (screenHeight - arrowHeight))
            .setRepeat(-1)
            .setOnComplete(ResetArrow);
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

    private float ArrowDirectionToDegrees(int direction)
    {
        // Default orientation (0 degrees) = 12 O' clock
        return direction * 90;
    }

    private void ResetArrow()
    {
        // Should be 0 when the arrow passes the top of the screen
        // and -0.5 when the arrow hits the mark
        // and less than -0.5 when the arrow was too early
        arrowHeight = arrowHeight - targetHeight - Mathf.Abs(startHeight);
        arrowDirection = (ArrowDirection)Random.Range(0, 4);
        ddrMaterial.SetFloat("_ArrowRotationDegrees", ArrowDirectionToDegrees((int)arrowDirection));
    }

    private void OnDestroy()
    {
        if (!gunController)
            return;
        if (!gunController.player)
            return;

        gunController.player.inputManager.onFirePerformed -= Fire;
        gunController.player.inputManager.onMovePerformed -= ArrowSelect;
    }
}
