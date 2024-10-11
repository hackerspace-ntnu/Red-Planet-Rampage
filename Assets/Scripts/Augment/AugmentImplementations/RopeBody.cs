using System.Linq;
using Mirror;
using Org.BouncyCastle.Asn1.Misc;
using UnityEngine;

public class RopeBody : GunBody
{
    [SerializeField]
    private Rope rope;
    [SerializeField]
    private Transform ropeTarget;
    private Vector3 previousTargetPosition = Vector3.zero;
    [SerializeField]
    private TetherPlug plugAnchorPrefab;
    private TetherPlug plugAnchor;
    [SerializeField]
    private GameObject handle;
    [SerializeField]
    private GameObject[] coils;
    private Rigidbody playerBody;
    private PlayerMovement movement;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private PlayerHand playerHandLeft;
    [SerializeField]
    private PlayerHand playerHandRight;
    [SerializeField]
    private AudioGroup plugPop;
    [SerializeField]
    private AudioGroup plugThrow;
    [SerializeField]
    private AudioGroup retractingWire;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private LayerMask ignoreLayer;

    [SerializeField]
    private float pullForce = 10f;
    [SerializeField]
    private float ropeLength = 60f;

    private bool isWired = false;
    private bool isThrowing = false;
    private bool canThrow = true;
    private float oldLength = 0f;

    private int throwTween;

    public override void Start()
    {
        base.Start();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController || !gunController.Player)
        {
            rope.enabled = false;
            return;
        }
        rope.Line.gameObject.layer = 0;
        rope.Target = ropeTarget;
        plugAnchor = Instantiate(plugAnchorPrefab);
        plugAnchor.transform.position = gunController.Player.transform.position;
        rope.Anchor = plugAnchor.transform;
        rope.ResetRope(plugAnchor.WireOrigin);
        plugAnchor.Health.onDeath += RemoveRope;
        playerBody = gunController.Player.GetComponent<Rigidbody>();
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandLeft.gameObject.SetActive(true);
        gunController.onFireNoAmmo += TryThrowPlug;
        movement = gunController.Player.GetComponent<PlayerMovement>();
        rope.enabled = false;
        plugAnchor.gameObject.SetActive(false);
        gunController.stats.Ammo = 0;
    }

    private void PullingWire()
    {
        if (!isWired || isThrowing || canThrow)
            return;

        if (!gunController.Player.inputManager)
        {
            // AIs should always break the rope, unless we want weird issues
            RemoveRope();
            return;
        }

        playerBody.AddForce(-(playerBody.position - rope.CurrentAnchor).normalized * pullForce, ForceMode.Acceleration);
        if (rope.RopeLength > ropeLength + 4f || (movement.StateIsAir && rope.RopeLength > ropeLength + 1f))
            RemoveRope();

    }

    private void RemoveRope()
    {
        plugAnchor.SetPulseStrength(0);
        isWired = false;
        canThrow = false;
        rope.CollisionCheckActive = false;
        int controlPointCount = Mathf.RoundToInt(Mathf.Max(rope.CollisionPoints.Count - 1f, 0f));
        gunController.stats.Ammo = 0;
        plugPop.Play(audioSource);
        float timePerPoint = rope.RopeLength / rope.CollisionPoints.Count * 0.05f;
        if (controlPointCount < 2)
        {
            AnimateLastVertex();
            rope.CollisionCheckActive = true;
            return;
        }

        LeanTween.value(gameObject, SetControlPointPullbackValue, 1f, 0f, timePerPoint)
            .setOnComplete(() =>
            {
                if (rope.CollisionPoints.Count > 1)
                    rope.CollisionPoints.RemoveAt(0);
                if (rope.CollisionPoints.Count < 2)
                {
                    rope.CollisionCheckActive = true;
                    AnimateLastVertex();
                }
                else
                {
                    retractingWire.Play(audioSource);
                }
            })
            .setOnCompleteOnRepeat(true)
            .setRepeat(controlPointCount);
    }

    private void RemoveRope(HealthController healthController, float damage, DamageInfo info)
    {
        if (isWired == false)
            return;
        RemoveRope();
    }

    private void TryThrowPlug(GunStats stats)
    {
        if (isWired || isThrowing || !canThrow)
            return;

        animator.SetTrigger("Plant");
        isThrowing = true;
    }

    // Called by animator
    // TODO Add some sanity checks once this game reaches popularity 😔
    public void ActivateWire()
    {
        if (!plugAnchor || !isOwned)
            return;

        var position = gunController.Player.inputManager
            ? gunController.Player.inputManager.transform.position
            : gunController.Player.GunHolder.transform.position;

        var direction = gunController.Player.inputManager
            ? gunController.Player.inputManager.transform.forward
            : Vector3.down;

        if (Physics.Raycast(position, direction, out RaycastHit hit, ropeLength, ignoreLayer))
        {
            StartThrowingWire(gunController.Player.transform.position, hit.normal);
            ThrowWireCmd(gunController.Player.transform.position, hit.point, hit.normal, hit.distance);

            if (LeanTween.isTweening(throwTween))
                LeanTween.cancel(throwTween);
            throwTween = plugAnchor.gameObject.LeanMove(hit.point, 0.02f * hit.distance)
                .setOnComplete(() =>
                {
                    PlaceWireCmd(hit.point, hit.normal);
                    AfterPlaceWire();
                }).id;
        }
        else
        {
            DeactivateWireCmd();
            DeactivateWire();
        }
    }

    private void StartThrowingWire(Vector3 position, Vector3 direction)
    {
        plugAnchor.transform.position = position;
        plugAnchor.transform.forward = direction;
        rope.enabled = true;
        canThrow = false;
        LeanTween.value(gameObject, UpdateRope, 0f, ropeLength, 0.02f * ropeLength);
        rope.ResetRope(plugAnchor.WireOrigin);
        plugAnchor.gameObject.SetActive(true);
    }

    [Command]
    private void ThrowWireCmd(Vector3 position, Vector3 target, Vector3 direction, float distance)
    {
        ThrowWireRpc(position, target, direction, distance);
    }

    [ClientRpc(includeOwner = false)]
    private void ThrowWireRpc(Vector3 position, Vector3 target, Vector3 direction, float distance)
    {
        StartThrowingWire(position, direction);

        if (LeanTween.isTweening(throwTween))
            LeanTween.cancel(throwTween);
        throwTween = plugAnchor.gameObject.LeanMove(target, 0.02f * distance).id;
    }

    [Command]
    private void PlaceWireCmd(Vector3 position, Vector3 normal)
    {
        PlaceWireRpc(position, normal);
    }

    [ClientRpc(includeOwner = false)]
    private void PlaceWireRpc(Vector3 position, Vector3 normal)
    {
        plugAnchor.transform.position = position;
        plugAnchor.transform.forward = normal;
        if (LeanTween.isTweening(throwTween))
            LeanTween.cancel(throwTween);
        AfterPlaceWire();
    }

    private void AfterPlaceWire()
    {
        rope.ResetRope(plugAnchor.WireOrigin);
        isThrowing = false;
        isWired = true;
        plugAnchor.SetPulseStrength(0.85f);
        gunController.stats.Ammo = gunController.stats.MagazineSize;
    }

    [Command]
    private void DeactivateWireCmd()
    {
        DeactivateWireRpc();
    }

    [ClientRpc(includeOwner = false)]
    private void DeactivateWireRpc()
    {
        DeactivateWire();
    }

    private void DeactivateWire()
    {
        plugAnchor.SetPulseStrength(0);
        plugAnchor.transform.position = gunController.Player.transform.position;
        if (gunController.Player.inputManager)
            plugAnchor.transform.forward = gunController.Player.inputManager.transform.forward;
        else
            plugAnchor.transform.forward = gunController.Player.GunHolder.transform.forward;
        rope.enabled = true;
        plugAnchor.gameObject.SetActive(true);
        AnimateFullPullBack();
    }

    public void PlayThrowAudio()
    {
        plugThrow.Play(audioSource);
    }

    private void AnimateFullPullBack()
    {
        LeanTween.value(gameObject, UpdateRope, 0f, ropeLength, 0.02f * ropeLength);

        var targetTransform = gunController.Player.inputManager?.transform ?? gunController.Player.GunHolder.transform;

        plugAnchor.gameObject.LeanMove(targetTransform.position + targetTransform.forward * ropeLength, 0.02f * ropeLength)
            .setOnComplete(() =>
            {
                retractingWire.Play(audioSource);
                rope.CollisionCheckActive = false;
                previousTargetPosition = plugAnchor.transform.position;
                LeanTween.value(gameObject, SetPullbackValue, 1f, 0f, 0.01f * ropeLength)
                .setOnComplete(() =>
                {
                    rope.enabled = false;
                    isThrowing = false;
                    plugAnchor.gameObject.SetActive(false);
                    canThrow = true;
                    rope.CollisionCheckActive = true;
                });
            });
    }
    private void AnimateLastVertex()
    {
        retractingWire.Play(audioSource);
        rope.CollisionCheckActive = false;
        previousTargetPosition = plugAnchor.transform.position;
        LeanTween.value(gameObject, SetPullbackValue, 1f, 0f, 0.01f * ropeLength)
            .setOnComplete(() =>
            {
                rope.enabled = false;
                isThrowing = false;
                plugAnchor.gameObject.SetActive(false);
                canThrow = true;
                rope.CollisionCheckActive = true;
            });
    }

    public void UpdateRope(float _)
    {
        rope.ResetRope(plugAnchor.WireOrigin);
    }

    public void SetPullbackValue(float value)
    {
        var finalTarget = coils[0].transform.position + -Vector3.up;
        var anchorDirection = finalTarget - previousTargetPosition;
        plugAnchor.transform.position = finalTarget - anchorDirection * value;
        plugAnchor.transform.forward = anchorDirection;
        rope.ResetRope(plugAnchor.WireOrigin);
    }

    public void SetControlPointPullbackValue(float value)
    {
        var point = rope.CollisionPoints.Count < 2 ? coils[0].transform.position : rope.CollisionPoints.FirstOrDefault();
        var direction = point - plugAnchor.transform.position;
        plugAnchor.transform.position = plugAnchor.transform.position + direction * value;
        plugAnchor.transform.forward = direction;
    }

    protected override void Reload(GunStats stats)
    {
        if (isWired)
            base.Reload(stats);
    }

    private void FixedUpdate()
    {
        if (!rope || !rope.enabled)
            return;
        var currentLength = rope.RopeLength;
        handle.transform.Rotate(Vector3.up, (oldLength - currentLength) * 40f);
        oldLength = currentLength;
        if (currentLength > ropeLength)
            PullingWire();
    }

    private void Update()
    {
        var cutOffIndex = Mathf.FloorToInt(oldLength / ropeLength * 11);
        for (int i = 0; i < coils.Length; i++)
            coils[i].SetActive(i > cutOffIndex);
    }

    private void OnDestroy()
    {
        if (!plugAnchor)
            return;
        plugAnchor.Health.onDeath -= RemoveRope;
        Destroy(plugAnchor);
        if (!gunController)
            return;
        gunController.onFireNoAmmo -= TryThrowPlug;
    }

}
