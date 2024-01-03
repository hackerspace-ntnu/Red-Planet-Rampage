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
        // TODO punch in, fade out or something and then destroy yourself
        var scale = transform.lossyScale;
        var sequence = LeanTween.sequence();
        sequence.append(LeanTween.scale(gameObject, scale * 1.5f, .4f).setEasePunch())
                .append(3)
                .append(LeanTween.scale(gameObject, Vector3.zero, .2f).setEaseOutQuad().setOnComplete(() => Destroy(gameObject)));
    }

    private void Update()
    {
        if (Camera)
            transform.rotation = Camera.rotation;
    }
}
