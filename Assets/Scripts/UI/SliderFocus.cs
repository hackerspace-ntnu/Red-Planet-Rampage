using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This class must be attached to every slider, It reselects the 
/// slider element after the player selects it.
/// </summary>
public class SliderFocus : MonoBehaviour, ISubmitHandler
{
    private MainMenuController menuController;

    public void OnSubmit(BaseEventData eventData)
    {
        if (menuController)
        {
            menuController.SelectControl(GetComponent<Slider>());
        }
    }

    private void Start()
    {
        menuController = GetComponentInParent<MainMenuController>();
    }
}
