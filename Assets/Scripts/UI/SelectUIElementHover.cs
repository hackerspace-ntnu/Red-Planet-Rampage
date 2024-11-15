using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

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
