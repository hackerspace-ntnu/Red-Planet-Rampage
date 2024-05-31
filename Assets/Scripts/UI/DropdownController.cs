using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Reselects unity dropdown elements when you enter a dropdown unity pls y u do dis. Add to every dropdown.
/// </summary>
public class DropdownController : MonoBehaviour, ISelectHandler
{
    private Selectable firstItem;

    public void OnSelect(BaseEventData eventData)
    {
        firstItem = GetComponentInChildren<Selectable>();
        StartCoroutine(WaitAndSelectFirstDropdownItem());
    }

    private IEnumerator WaitAndSelectFirstDropdownItem()
    {
        yield return new WaitForEndOfFrame();
        firstItem.Select();
    }

}
