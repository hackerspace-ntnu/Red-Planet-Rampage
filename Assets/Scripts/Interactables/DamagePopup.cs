using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private float lowDamage = 20;

    [SerializeField]
    private float highDamage = 80;

    [SerializeField]
    private Color normalLowColor;

    [SerializeField]
    private Color normalHighColor;

    [SerializeField]
    private Color criticalLowColor;

    [SerializeField]
    private Color criticalHighColor;

    public Transform Camera { get; set; }

    public float Damage
    {
        set
        {
            label.text = Mathf.RoundToInt(value).ToString();
        }
        get
        {
            return int.Parse(label.text);
        }
    }

    // TODO make more flashy if crit!
    public bool IsCritical { get; set; }

    private void Start()
    {
        label.color = DetermineColor();

        var scale = transform.lossyScale;
        LeanTween.sequence()
            .append(LeanTween.scale(gameObject, scale * 2f, .4f).setEasePunch())
            .insert(LeanTween.moveY(gameObject, transform.position.y + 1f, 1).setEaseInOutExpo())
            .append(LeanTween.scale(gameObject, Vector3.zero, .2f).setEaseOutQuad()
            .setOnComplete(() => Destroy(gameObject)));
    }

    private Color DetermineColor() =>
        IsCritical
            ? Color.Lerp(criticalLowColor, criticalHighColor, (Damage - lowDamage) / highDamage)
            : Color.Lerp(normalLowColor, normalHighColor, (Damage - lowDamage) / highDamage);


    private void Update()
    {
        if (Camera)
            transform.rotation = Camera.rotation;
    }
}
