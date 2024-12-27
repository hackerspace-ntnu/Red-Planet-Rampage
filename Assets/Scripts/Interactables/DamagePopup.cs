using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private float scaleMultiplier = 2f;

    [SerializeField]
    private bool isHUD = false;

    public Transform Camera { get; set; }

    private float lastDamage;

    public float Damage
    {
        set
        {
            label.text = isHUD 
                ? $"-{Mathf.RoundToInt(value)}%"
                : Mathf.RoundToInt(value).ToString();
            lastDamage = value;
        }
        get
        {
            return lastDamage;
        }
    }

    public bool IsCritical { get; set; }

    private void Start()
    {
        if (IsCritical && !isHUD)
        {
            LeanTween.value(gameObject, (Color value) => label.color = value, ColorPallete.Health(1), ColorPallete.Health(0.2f), 0.5f).setEasePunch();
            GetComponent<CriticalTextAnimator>().IsAnimated = true;
        }
        else
            LeanTween.value(gameObject, (Color value) => label.color = value, Color.white, ColorPallete.Health(0.2f), 1f).setEasePunch();

        var scale = transform.lossyScale;
        if (isHUD)
            LeanTween.sequence()
                .append(LeanTween.moveLocalY(gameObject, transform.localPosition.y + 1, .2f).setEaseInOutExpo())
                .append(LeanTween.scale(gameObject, scale * scaleMultiplier, .4f).setEasePunch())
                .insert(LeanTween.moveLocalY(gameObject, transform.localPosition.y + 2, 1).setEaseInOutExpo())
                .append(LeanTween.scale(gameObject, Vector3.zero, .2f).setEaseOutQuad()
                .setOnComplete(() => Destroy(gameObject)));
        else
            LeanTween.sequence()
                .append(LeanTween.moveY(gameObject, transform.position.y + 1f, .2f).setEaseInOutExpo())
                .append(LeanTween.scale(gameObject, scale * scaleMultiplier, .4f).setEasePunch())
                .insert(LeanTween.moveY(gameObject, transform.position.y + 2f, 1).setEaseInOutExpo())
                .append(LeanTween.scale(gameObject, Vector3.zero, .2f).setEaseOutQuad()
                .setOnComplete(() => Destroy(gameObject)));
    }

    private void Update()
    {
        if (Camera)
            transform.rotation = Camera.rotation;
    }
}
