using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class RopeBody : GunBody
{
    [SerializeField]
    private Rope rope;
    [SerializeField]
    private Transform ropeTarget;
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
    private VisualEffect dustOff;
    [SerializeField]
    private PlayerHand playerHandLeft;
    [SerializeField]
    private PlayerHand playerHandRight;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private LayerMask ignoreLayer;

    [SerializeField]
    private float pullForce = 10f;
    [SerializeField]
    private float ropeLength = 60f;

    private bool isWired = true;
    private bool isThrowing = false;
    private float oldLength = 0f;

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
        gunController.onFireNoAmmo += CheckForWirePlanting;
        movement = gunController.Player.GetComponent<PlayerMovement>();
    }

    private void PullingWire()
    {
        playerBody.AddForce(-(playerBody.position - rope.CurrentAnchor).normalized * pullForce, ForceMode.Acceleration);
        if (rope.RopeLength > ropeLength + 4f || (movement.StateIsAir && rope.RopeLength > ropeLength + 1f))
            RemoveRope();

    }

    private void RemoveRope()
    {
        isWired = false;
        rope.enabled = false;
        gunController.stats.Ammo = 0;
        plugAnchor.gameObject.SetActive(false);
        audioSource.Play();
    }

    private void RemoveRope(HealthController healthController, float damage, DamageInfo info)
    {
        isWired = false;
        rope.enabled = false;
        gunController.stats.Ammo = 0;
        plugAnchor.gameObject.SetActive(false);
        audioSource.Play();
    }

    private void CheckForWirePlanting(GunStats stats)
    {
        if (isWired || isThrowing)
            return;

        animator.SetTrigger("Plant");
        isThrowing = true;
    }

    // Called by animator
    public void ActivateWire()
    {
        if (!plugAnchor)
            return;
        if (Physics.Raycast(gunController.Player.inputManager.transform.position, gunController.Player.inputManager.transform.forward, out RaycastHit hit, ropeLength, ignoreLayer))
        {
            plugAnchor.transform.position = gunController.Player.transform.position;
            plugAnchor.transform.forward = hit.normal;
            rope.enabled = true;
            LeanTween.value(gameObject, SetThrowValue, 0f, ropeLength, 0.02f * ropeLength);
            plugAnchor.gameObject.LeanMove(hit.point, 0.02f * hit.distance)
                .setOnComplete(() =>
                {
                    rope.ResetRope(plugAnchor.WireOrigin);
                    isThrowing = false;
                    isWired = true;
                    gunController.stats.Ammo = gunController.stats.MagazineSize;
                });
            rope.ResetRope(plugAnchor.WireOrigin);
            plugAnchor.gameObject.SetActive(true);
        }
        else
        {
            plugAnchor.transform.position = gunController.Player.transform.position;
            plugAnchor.transform.forward = gunController.Player.inputManager.transform.forward;
            rope.enabled = true;
            plugAnchor.gameObject.SetActive(true);
            LeanTween.value(gameObject, SetThrowValue, 0f, ropeLength, 0.02f * ropeLength)
                .setEaseOutElastic();
            plugAnchor.gameObject.LeanMove(gunController.Player.inputManager.transform.position + gunController.Player.inputManager.transform.forward * ropeLength, 0.02f * ropeLength)
                .setOnComplete(() =>
                {
                    LeanTween.value(gameObject, SetPullbackValue, 1f, 0f, 1f)
                    .setOnComplete(() =>
                    {
                        rope.enabled = false;
                        isThrowing = false;
                        plugAnchor.gameObject.SetActive(false);
                    });
                });
        }
    }

    public void SetThrowValue(float value)
    {
        rope.ResetRope(plugAnchor.WireOrigin);
    }

    public void SetPullbackValue(float value)
    {
        // TODO: pullback through every control point of the rope
        plugAnchor.transform.position = ropeTarget.position + (ropeTarget.position - plugAnchor.transform.position) * value;
        rope.ResetRope(plugAnchor.WireOrigin);
    }

    public void PlayVFX()
    {
        dustOff.Play();
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
    }

}
