using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraRaycastController : MonoBehaviour
{
    [SerializeField]
    private MainMenuController mainMenuController;
    private Selectable lastUIElement;

    void Update()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.layer != 2)
                {
                    if (result.gameObject.TryGetComponent<Selectable>(out var selectable) && (lastUIElement == null || !lastUIElement.Equals(selectable)))
                    {
                        mainMenuController.SelectControl(selectable);
                        lastUIElement = selectable;
                    }
                }
            }
        }
        else
        {
            mainMenuController.SelectControl(null);
            EventSystem.current.SetSelectedGameObject(null);
            lastUIElement = null;
        }
    }
}
