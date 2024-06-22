using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderValue : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TMP_Text valueLabel;

    private void Start()
    {
        OnValueChanged(slider.value);
    }

    public void OnValueChanged(float value)
    {
        valueLabel.text = ((int)value).ToString();

    }
}
