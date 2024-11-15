using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    private Selectable button;
    public Selectable Button => button;
    [SerializeField]
    private TMP_Text checkText;
    [SerializeField]
    private bool isToggled = false;
    [SerializeField]
    private TMP_Text highlightPopup;

    void Start()
    {
        button = GetComponent<Selectable>();
        checkText.enabled = isToggled;
        if (highlightPopup)
            highlightPopup.enabled = false;
    }

    public void OnHighlight()
    {
        if (isToggled && highlightPopup)
            highlightPopup.enabled = true;
    }

    public void OnRemoveHighlight()
    {
        if (highlightPopup)
            highlightPopup.enabled = isToggled;
    }

    public void Toggle()
    {
        isToggled = !isToggled;
        if (isToggled && highlightPopup)
            highlightPopup.enabled = true;
        checkText.enabled = isToggled;
    }

    public void SetIsToggled(bool value)
    {
        isToggled = value;
        if (isToggled && highlightPopup)
            highlightPopup.enabled = true;
        checkText.enabled = isToggled;
    }
}
