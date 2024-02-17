using System.Collections;
using System.Collections.Generic;
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
    private float rotationAngle;
    [SerializeField]
    private GameObject directionalLight;

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

        LeanTween.sequence()
            .append(LeanTween.rotateY(mainMenuCamera.gameObject, 120, cameraSpeed).setEaseInOutQuart());
    }

    public void MoveToPlayerSelect()
    {
        playerSelectCamera.gameObject.SetActive(true);
        mainMenuCamera.gameObject.SetActive(false);
        directionalLight.SetActive(false);
    }
}
