using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMoveCamera : MonoBehaviour
{
    [SerializeField]
    private Camera mainMenuCamera;
    [SerializeField]
    private float cameraSpeed;
    [SerializeField]
    private float rotationAngle;
    [SerializeField]
    private Transform cameraPositionDefault;
    [SerializeField]
    private Transform cameraPositionAlternative;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //
    public void MoveToOptions()
    {
        LeanTween.sequence()
            .append(LeanTween.rotateY(mainMenuCamera.gameObject, 10, cameraSpeed).setEaseInOutQuart());
    }

    public void MoveToDefault()
    {
        LeanTween.sequence()
            .append(LeanTween.rotateY(mainMenuCamera.gameObject, 120, cameraSpeed).setEaseInOutQuart());
    }
}
