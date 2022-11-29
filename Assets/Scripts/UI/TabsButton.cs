using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class TabsButton : MonoBehaviour, ISelectHandler
{
    public TabGroup tabGroup;

    public Image background;

    public GameObject tab;

    public void OnSelect(BaseEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    private void Start()
    {
        background = GetComponent<Image>();
        tabGroup.Subscribe(this);
    }
}
