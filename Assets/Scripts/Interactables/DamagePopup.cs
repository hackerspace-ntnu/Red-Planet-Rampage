using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;

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

    private void Start()
    {
        var scale = transform.lossyScale;
        LeanTween.sequence()
            .append(LeanTween.scale(gameObject, scale * 2f, .4f).setEasePunch())
            .insert(LeanTween.moveY(gameObject, transform.position.y + 1f, 1).setEaseInOutExpo())
            .append(LeanTween.scale(gameObject, Vector3.zero, .2f).setEaseOutQuad()
            .setOnComplete(() => Destroy(gameObject)));
    }

    private void Update()
    {
        if (Camera)
            transform.rotation = Camera.rotation;
    }
}
