using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VectorExtensions;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private LayerMask cullingMask;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private Transform initialTarget;
    [SerializeField] private float maxRadius = 5;
    [SerializeField] private float minRadius = 1;
    [SerializeField] private float collisionOffset = .3f;

    public InputManager InputManager;

    private Transform cameraTransform;
    private new Camera camera;

    public Camera Camera
    {
        set
        {
            camera = value;
            camera.cullingMask = cullingMask;
            cameraTransform = value.transform;
        }
    }

    private Transform target;

    private bool isTracking;
    private Vector2 aimAngle;
    private float radius;

    private PlayerManager[] otherPlayers;
    private int targetIndex = 0;

    private void Start()
    {
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += StopTracking;
    }

    private void OnDestroy()
    {
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= StopTracking;
        if (!cameraTransform)
            return;
        StopTracking();
    }


    public void Activate()
    {
        StartTracking(initialTarget);
        if (!MatchController.Singleton.IsRoundInProgress)
            return;
        otherPlayers = MatchController.Singleton.Players.Where(p => p != GetComponent<PlayerManager>()).ToArray();
        StartCoroutine(WaitAndStopTrackingRagdoll());
    }

    private IEnumerator WaitAndStopTrackingRagdoll()
    {
        yield return new WaitForSeconds(7f);
        StopTracking();
        GetComponent<PlayerManager>().HUDController.HideDeathScreen();
        InputManager.onSelect += SwitchTarget;
    }

    private void SwitchTarget(InputAction.CallbackContext ctx)
    {
        if (!MatchController.Singleton.IsRoundInProgress)
            return;

        if (targetIndex >= otherPlayers.Length)
            StopTracking();
        else
            StartTracking(otherPlayers[targetIndex].AiAimSpot);

        targetIndex = (targetIndex + 1) % (otherPlayers.Length + 1);
    }

    private void StartTracking(Transform nextTarget)
    {
        if (!cameraTransform || !InputManager || !MatchController.Singleton.IsRoundInProgress)
            return;
        isTracking = true;
        camera.enabled = true;
        target = nextTarget;
    }

    private void StopTracking()
    {
        if (!camera)
            return;
        isTracking = false;
        camera.enabled = false;
        ResetCamera();
    }

    private void ResetCamera()
    {
        cameraTransform.position = Vector3.zero;
        cameraTransform.localPosition = Vector3.zero;
        cameraTransform.rotation = Quaternion.identity;
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
        // Polar coordinates for position on unit sphere
        var orbitPosition = new Vector3(Mathf.Cos(aimAngle.x), Mathf.Sin(aimAngle.y), MathF.Sin(aimAngle.x));

        // Determine radius based on collision
        var rawRadius = maxRadius;
        if (Physics.Raycast(target.position, orbitPosition, out var hit, maxRadius, collisionMask))
            rawRadius = Mathf.Clamp(hit.distance - collisionOffset, minRadius, maxRadius);
        radius = Mathf.Lerp(radius, rawRadius, 10 * Time.deltaTime);

        cameraTransform.position = target.position + radius * orbitPosition;
        cameraTransform.LookAt(target);
    }

    private void Update()
    {
        if (!isTracking || !cameraTransform)
            return;

        UpdateAngles();
        UpdateOrbit();
    }
}
