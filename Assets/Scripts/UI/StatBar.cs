using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform baseRect;

    [SerializeField]
    private RectTransform deltaRect;

    [SerializeField]
    private Color baseColor;

    [SerializeField]
    private Color negativeColor;

    [SerializeField]
    private Color positiveColor;

    [SerializeField]
    private float maxWidth = 100;

    [SerializeField]
    private float defaultMaxValue = 30;

    private float currentMaxValue;

    private RawImage deltaImage;

    [SerializeField]
    private float tweenDuration = .07f;

    [SerializeField]
    private float baseValue = 50;
    public float BaseValue
    {
        get { return baseValue; }
        set
        {
            baseValue = value;
            UpdateMeter();
        }
    }

    [SerializeField]
    private float newValue = 20;
    public float NewValue
    {
        get { return newValue; }
        set
        {
            newValue = value;
            UpdateMeter();
        }
    }

    void Awake()
    {
        deltaImage = deltaRect.GetComponent<RawImage>();
        baseRect.GetComponent<RawImage>().color = baseColor;
        currentMaxValue = defaultMaxValue;

        UpdateMeter();
    }

    private void UpdateMeter()
    {
        // Avoid one peculiar bug where the RectTransform is null
        // and the MissingReferenceException spam prevents the next scene from loading
        if (baseRect == null)
        {
            Debug.LogWarning("yeah it almost happened");
            return;
        }

        float height = baseRect.sizeDelta.y;

        // Set new max if we need to
        currentMaxValue = Mathf.Max(currentMaxValue, Mathf.Max(newValue, baseValue));

        // Interpolate normalized value
        float newLerpedValue = Mathf.Lerp(0, maxWidth, newValue / currentMaxValue);
        float baseLerpedValue = Mathf.Lerp(0, maxWidth, baseValue / currentMaxValue);

        if (newValue < baseValue)
        {
            // Negative delta; base will have delta take a slice out of it
            deltaImage.color = negativeColor;
            LeanTween.size(baseRect, new Vector2(newLerpedValue, height), tweenDuration);
            LeanTween.size(deltaRect, new Vector2(baseLerpedValue - newLerpedValue, height), tweenDuration);
        }
        else
        {
            // Positive (or zero) delta; base and delta purely display their value
            if (newValue > baseValue) deltaImage.color = positiveColor;
            LeanTween.size(baseRect, new Vector2(baseLerpedValue, height), tweenDuration);
            LeanTween.size(deltaRect, new Vector2(newLerpedValue - baseLerpedValue, height), tweenDuration);
        }
    }
}
