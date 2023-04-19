using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{

    [SerializeField] 
    private GameObject selectedGameObject;
    
    public void SetPlayerInput(InputManager input)
    {
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
        EventSystem.current.SetSelectedGameObject(selectedGameObject);
    }

}
