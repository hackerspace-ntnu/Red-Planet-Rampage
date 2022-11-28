using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropdownController : MonoBehaviour, ISelectHandler
{
    private ScrollRect scrollRect;
    private float scrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>(true);

        int childCount = scrollRect.content.childCount - 1;
        int childIndex = transform.GetSiblingIndex();

        childIndex = childIndex < ((float)childCount / 2f) ? childIndex - 1 : childIndex;

        scrollPosition = 1 - ((float)childIndex / childCount);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(scrollRect)
            scrollRect.verticalScrollbar.value = scrollPosition;
    }

    public void Changed()
    {
        Debug.Log("Value changed");
    }
}
