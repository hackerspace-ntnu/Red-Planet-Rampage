using UnityEngine;

public class MainMenuMoveCamera : MonoBehaviour
{
    [SerializeField]
    private Camera mainMenuCamera;
    [SerializeField]
    private Camera playerSelectCamera;
    [SerializeField]
    private float cameraSpeed;
    [SerializeField]
    private GameObject directionalLight;

    [SerializeField]
    private Transform initialPosition;
    [SerializeField]
    private Transform secondPosition;

    private bool inLevelSelect;

    public void MoveToOptions()
    {
        LeanTween.sequence()
            .append(LeanTween.rotateY(mainMenuCamera.gameObject, 10, cameraSpeed).setEaseInOutQuart());
    }

    public void MoveToDefault()
    {
        playerSelectCamera.gameObject.SetActive(false);
        mainMenuCamera.gameObject.SetActive(true);
        directionalLight.SetActive(true);

        // Have to turn cursor back to visible as it seems it turns into invisible when switching cam.
        Cursor.visible = true;

        LeanTween.sequence()
            .append(LeanTween.rotateY(mainMenuCamera.gameObject, 120, cameraSpeed).setEaseInOutQuart());
    }

    public void MoveToPlayerSelect()
    {
        playerSelectCamera.gameObject.SetActive(true);
        mainMenuCamera.gameObject.SetActive(false);
        directionalLight.SetActive(false);

        if (inLevelSelect)
        {
            inLevelSelect = false;
            LeanTween.sequence().append(LeanTween.moveLocal(playerSelectCamera.gameObject, initialPosition.localPosition, 1).setEaseInOutExpo());
            LeanTween.sequence().append(LeanTween.rotateX(playerSelectCamera.gameObject, 9f, 1).setEaseInOutExpo());
        }
    }

    public void MoveToLevelSelect()
    {
        if (!(PlayerInputManagerController.Singleton.MatchHasAI || PlayerInputManagerController.Singleton.PlayerCount > 1))
            return;
        inLevelSelect = true;
        LeanTween.sequence().append(LeanTween.moveLocal(playerSelectCamera.gameObject, secondPosition.localPosition, 1).setEaseInOutExpo());
        LeanTween.sequence().append(LeanTween.rotateX(playerSelectCamera.gameObject, 90f, 1).setEaseInOutExpo());
    }
}
