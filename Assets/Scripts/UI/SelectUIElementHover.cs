using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectUIElementHover : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(WaitSelect());
    }

    private IEnumerator WaitSelect()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
