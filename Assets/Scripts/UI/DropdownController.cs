using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Allows for our controllers to freely act in dropdown menus
/// </summary>
public class DropdownController : MonoBehaviour, ISelectHandler
{
    private ScrollRect scrollRect;
    //private TMPro.TMP_ScrollbarEventHandler eventHandler;
    private float scrollPosition;

    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>(true);
        
        
        int childCount = scrollRect.content.childCount - 1;
        int childIndex = transform.GetSiblingIndex();

        // Slightly adjusts the child index to prevent ugly cutoff when at the extremities.
        childIndex = childIndex < ((float)childCount / 2f) ? childIndex - 1 : childIndex;

        scrollPosition = 1 - ((float)childIndex / childCount);
    }

    public void OnSelect(BaseEventData eventData)
    {
        //GetComponentInParent<MainMenuController>().SelectControl(GetComponentInChildren<Selectable>());

        if(scrollRect)
            scrollRect.verticalScrollbar.value = scrollPosition;
    
    }
}
