using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Reselects unity dropdown elements when you enter a dropdown unity pls y u do dis. Add to every dropdown.
/// </summary>
public class UIReselector : MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(WaitAndSelectFirstDropdownItem());
    }

    private IEnumerator WaitAndSelectFirstDropdownItem()
    {
        yield return new WaitForEndOfFrame();
        GetComponentInChildren<Selectable>().Select();
    }

}
