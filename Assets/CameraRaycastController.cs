using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraRaycastController : MonoBehaviour
{
    [SerializeField]
    private MainMenuController menuController;
    private Selectable lastUIElement;

    void Update()
    {
        PointerEventData pointerData = new(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {   
                if (result.gameObject.layer == 5)
                {
                    // TabButtons are expected to have a TabGroup parent
                    if (result.gameObject.TryGetComponent<TabsButton>(out var tabButton))
                    {
                        TabGroup tabParent = tabButton.GetComponentInParent<TabGroup>();

                        //Maybe replace GetMouseButtonDown for something else?
                        if (tabParent != null && Input.GetMouseButtonDown(0))
                        {
                            tabParent.SelectTab(tabButton);
                        }
                    }
                    else if (result.gameObject.TryGetComponent<Selectable>(out var selectable) && (lastUIElement == null || !lastUIElement.Equals(selectable)))
                    {
                        menuController.SelectControl(selectable);
                        lastUIElement = selectable;
                    }
                }
            }
        }
        else
        {
            menuController.SelectControl(null);
            EventSystem.current.SetSelectedGameObject(null);
            lastUIElement = null;
        }
    }
}
