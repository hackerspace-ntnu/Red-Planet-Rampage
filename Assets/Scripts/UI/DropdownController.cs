using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

/// <summary>
/// Allows for our controllers to freely act in dropdown menus
/// </summary>
public class DropdownController : MonoBehaviour, ISelectHandler
{
    [SerializeField]
    private Selectable[] selectables;
    [SerializeField]
    private TMP_Dropdown scrollbar;
    [SerializeField]
    private Selectable CurrentSelected;

    void Start()
    {
        scrollbar = GetComponent<TMP_Dropdown>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        try
        {
            if (scrollbar.IsExpanded) {
                selectables = GetComponentsInChildren<Selectable>();
                print(selectables.Length);
            }
        }
        catch 
        {
            selectables = GetComponentsInChildren<Selectable>();
            print(selectables[0].name);
            print("selected gameobject: " + EventSystem.current.currentSelectedGameObject.name);
            CurrentSelected = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            print(CurrentSelected.navigation.selectOnDown.name);

            selectables[0].Select();
            //StartCoroutine("UIDelay");
            //scrollbar.FindSelectableOnDown().Select();
        }
    }

    private IEnumerator UIDelay()
    {
        yield return new WaitForEndOfFrame();
        selectables = GetComponentsInChildren<Selectable>();
        print(selectables.Length);
        if (selectables.Length > 1)
            yield return null;
        StartCoroutine("UIDelay");
    }
}
