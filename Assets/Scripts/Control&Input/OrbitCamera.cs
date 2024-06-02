using System;
using UnityEngine;

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
        // TODO create an extension method or something?
        // Constrain aiming angle vertically and wrap horizontally.
        // + and - Mathf.Deg2Rad is offsetting with 1 degree in radians,
        // which is neccesary to avoid IK shortest path slerping that causes aniamtions to break at exactly the halfway points.
        // This is way more computationaly efficient than creating edgecase checks in IK with practically no gameplay impact
        aimAngle.y = Mathf.Clamp(aimAngle.y, -Mathf.PI / 2 + Mathf.Deg2Rad, Mathf.PI / 2 - Mathf.Deg2Rad);
        aimAngle.x = (aimAngle.x + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
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
