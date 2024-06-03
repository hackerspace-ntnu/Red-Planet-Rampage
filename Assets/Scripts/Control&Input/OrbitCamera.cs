using System;
using UnityEngine;
using VectorExtensions;

public class OrbitCamera : MonoBehaviour
{
    public InputManager InputManager { get; set; }

    public Transform Camera { get; set; }

    private Transform target;

    private bool isTracking;
    private Vector2 aimAngle;

    // TODO adjust based on raycasting for obstacles
    private float radius = 5;

    public void StartTracking(Transform target)
    {
        if (!Camera || !InputManager)
            return;
        isTracking = true;
        this.target = target;
    }

    public void StopTracking()
    {
        isTracking = false;
        ResetCamera();
    }

    private void ResetCamera()
    {
        Camera.position = Vector3.zero;
        Camera.localPosition = Vector3.zero;
        Camera.rotation = Quaternion.identity;
        InputManager.transform.rotation = Quaternion.identity;
    }

    private void UpdateAngles()
    {
        var lookInput = InputManager.IsMouseAndKeyboard
            ? InputManager.lookInput
            : InputManager.lookInput * Time.deltaTime;
        aimAngle += lookInput * 10; // TODO idk set this properly
        aimAngle = aimAngle.ClampedLookAngles();
    }

    private void UpdateOrbit()
    {
        var orbitPosition = new Vector3(Mathf.Cos(aimAngle.x), Mathf.Sin(aimAngle.y), MathF.Sin(aimAngle.x));
        Camera.position = target.position + radius * orbitPosition;
        Camera.LookAt(target);
    }

    private void Update()
    {
        if (!isTracking || !Camera)
            return;

        UpdateAngles();
        UpdateOrbit();
    }

    private void OnDestroy()
    {
        if (!Camera)
            return;
        ResetCamera();
    }
}
