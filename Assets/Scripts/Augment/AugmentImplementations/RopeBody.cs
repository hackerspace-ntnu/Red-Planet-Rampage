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
    private float pullForce = 10f;
    [SerializeField]
    private float ropeLength = 60f;

    private bool isWired = true;

    public override void Start()
    {
        base.Start();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController || !gunController.Player)
        {
            rope.enabled = false;
            return;
        }
        rope.Target = ropeTarget;
        plugAnchor = Instantiate(plugAnchorPrefab);
        plugAnchor.transform.position = gunController.Player.transform.position;
        rope.Anchor = plugAnchor.transform;
        rope.ResetRope(plugAnchor.WireOrigin);
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
        if (rope.RopeLength > ropeLength + 4f)
        {
            isWired = false;
            rope.enabled = false;
            gunController.stats.Ammo = 0;
            plugAnchor.gameObject.SetActive(false);
        }
            
    }

    private void CheckForWirePlanting(GunStats stats)
    {
        if (isWired || movement.StateIsAir)
            return;
        rope.enabled = true;
        plugAnchor.transform.position = gunController.Player.transform.position;
        movement.CanMove = false;
        movement.CanLook = false;
        animator.SetTrigger("Plant");
        isWired = true;
        rope.ResetRope(plugAnchor.WireOrigin);
    }

    // Called by animator
    public void ActivateWire()
    {
        if (!plugAnchor)
            return;
        plugAnchor.gameObject.SetActive(true);
        movement.CanMove = true;
        movement.CanLook = true;
        gunController.stats.Ammo = gunController.stats.MagazineSize;
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
        if (rope.RopeLength > ropeLength)
            PullingWire();
    }

    private void OnDestroy()
    {
        Destroy(plugAnchor);
    }

}
