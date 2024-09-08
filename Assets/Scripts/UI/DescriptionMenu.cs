using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DescriptionMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_Text description;

    public void SetTitle(string text)
    {
        title.text = text;
    }

    public void SetDescription(string text)
    {
        description.text = text;
    }
}
