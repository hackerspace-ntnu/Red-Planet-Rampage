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
    [SerializeField] private float maxRadius = 5;
    [SerializeField] private float minRadius = 1;
    [SerializeField] private float collisionOffset = .3f;
    [SerializeField] private float lookSensitivity = 5;

    private InputManager input;

    private Transform cameraTransform;
    private new Camera camera;

    private Transform target;

    private bool isTracking;
    private Vector2 aimAngle;
    private float radius;

    private PlayerManager[] otherPlayers;
    private int targetIndex = 0;
    private PlayerManager player;

    private void Start()
    {
        player = GetComponent<PlayerManager>();
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


    public void Activate(InputManager input)
    {
        if (MatchController.Singleton.IsShowingScoreboards)
            return;

        this.input = input;
        camera = input.PlayerCamera;
        camera.cullingMask = cullingMask | (1 << (12 + player.LayerIndex));
        cameraTransform = camera.transform;

        StartTracking(player);
        otherPlayers = MatchController.Singleton.Players.Where(p => p != GetComponent<PlayerManager>()).ToArray();
        StartCoroutine(WaitAndStopTrackingRagdoll());
    }

    private IEnumerator WaitAndStopTrackingRagdoll()
    {
        yield return new WaitForSeconds(3f);
        player.HUDController.DisplaySpectateHint();
        input.onSelect += SwitchTarget;
        input.onFirePerformed += SwitchTarget;
        yield return new WaitForSeconds(3f);
        var isStillOnPlayer = target == player.AiAimSpot;
        if (isStillOnPlayer)
        {
            StopTracking();
            // Will focus on next player next time
            targetIndex = 1;
        }
    }

    private void SwitchTarget(InputAction.CallbackContext ctx)
    {
        if (MatchController.Singleton.IsShowingScoreboards)
            return;

        // 0 is a special index, and the remainder is i+1 so we can safely subtract.
        if (targetIndex == 0)
            StopTracking();
        else
            StartTracking(otherPlayers[targetIndex - 1]);

        targetIndex = (targetIndex + 1) % (otherPlayers.Length + 1);
    }

    private void StartTracking(PlayerManager nextTarget)
    {
        if (!cameraTransform || !input || MatchController.Singleton.IsShowingScoreboards)
            return;
        isTracking = true;
        camera.enabled = true;
        target = nextTarget.AiAimSpot;
        if (nextTarget != player)
            player.HUDController.DisplaySpectatorScreen(nextTarget.identity);
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
        input.transform.rotation = Quaternion.identity;
    }

    private void UpdateAngles()
    {
        var lookInput = input.IsMouseAndKeyboard
            ? input.lookInput
            : input.lookInput * Time.deltaTime;
        aimAngle += lookInput * lookSensitivity; // TODO idk set this properly
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
