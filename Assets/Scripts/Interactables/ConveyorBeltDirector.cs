using UnityEngine;

public class ConveyorBeltDirector : MonoBehaviour
{
    private static float direction = 1;
    private static float directionMultiplier = 1;
    public static float DirectionMultiplier => directionMultiplier;

    [SerializeField]
    private FlippableSwitch flippableSwitch;

    [SerializeField]
    private Renderer leftArrow;

    [SerializeField]
    private Renderer rightArrow;

    [SerializeField]
    private Speedometer speedometer;

    private int flipTween;

    private void Start()
    {
        flippableSwitch.OnFlip += Flip;

        SetDirectionMultiplier(direction);
        FlipArrows();
    }

    private void Flip(bool _)
    {
        direction = -direction;
        FlipDirectionMultiplier();
        FlipArrows();
    }

    private void FlipDirectionMultiplier()
    {
        if (LeanTween.isTweening(flipTween))
            LeanTween.cancel(flipTween);
        flipTween = LeanTween.value(gameObject, SetDirectionMultiplier, directionMultiplier, direction, 1f).id;
    }

    private void SetDirectionMultiplier(float value)
    {
        directionMultiplier = value;
        speedometer.SetDisplayedValue((1 - value) / 2f);
    }

    private void FlipArrows()
    {
        leftArrow.enabled = direction > 0;
        rightArrow.enabled = direction < 0;
    }
}
