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
    private float displayScale = 1;

    private RawImage deltaImage;

    [SerializeField]
    private float tweenDuration = .07f;

    [SerializeField]
    private float baseValue = 100;
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

        UpdateMeter();
    }

    private void UpdateMeter()
    {
        float height = baseRect.sizeDelta.y;
        if (newValue < baseValue)
        {
            // Negative delta; base will have delta take a slice out of it
            deltaImage.color = negativeColor;
            LeanTween.size(baseRect, new Vector2(newValue * displayScale, height), tweenDuration);
            LeanTween.size(deltaRect, new Vector2((baseValue - newValue) * displayScale, height), tweenDuration);
        }
        else
        {
            // Positive (or zero) delta; base and delta purely display their value
            if (newValue > baseValue) deltaImage.color = positiveColor;
            LeanTween.size(baseRect, new Vector2(baseValue * displayScale, height), tweenDuration);
            LeanTween.size(deltaRect, new Vector2((newValue - baseValue) * displayScale, height), tweenDuration);
        }
    }
}
