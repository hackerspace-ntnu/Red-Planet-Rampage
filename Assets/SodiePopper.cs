using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SodiePopper : GunBody
{

    [SerializeField]
    float offsetMagnitude = 0.1f;

    // Prevents the players movement from being too dominant in the shaking
    [SerializeField]
    float playermovementRelativeAdjustment = 0.4f;

    Vector3 lastPos = Vector3.zero;
    Vector3 lastLastPos = Vector3.zero;

    [SerializeField]
    float shakeCorrectionReloadThreshold = 0.003f;

    [SerializeField]
    float reload_delay = 0.1f;

    [SerializeField]
    float reload_ammount = 0.2f;

    float lastReload = 0f;

    [SerializeField]
    float velocityDamping = 0.1f;
    [SerializeField]
    float springStrength = 0.1f;

    [SerializeField]
    Transform meassurementPoint = null;

    [SerializeField]
    Transform offsetTarget = null;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    private PlayerHand playerHandRight;

    private float lastDiff;

    private Rigidbody playerBody;

    private float lastPlayerDiff = 0f;

    private AudioSource audioSource;

    [SerializeField]
    private AudioGroup slosh;

    public override void Start()
    {
        gunController = transform.parent?.GetComponent<GunController>();

        lastPos = lastLastPos = meassurementPoint.position;

        if (!gunController)
            return;
        gunController.HasRecoil = false;

        if (!gunController.Player)
            return;
        audioSource = GetComponent<AudioSource>();
        playerBody = gunController.Player.GetComponent<PlayerMovement>().Body;
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (gunController != null)
        {
            anim.SetFloat("Ammo", (float)gunController.stats.Ammo / (float)gunController.stats.MagazineSize);
        }
    }

    private void FixedUpdate()
    {
        Vector3 offsetVelocity = (lastPos - lastLastPos) / Time.fixedDeltaTime;

        offsetVelocity -= offsetVelocity * velocityDamping * Time.fixedDeltaTime;

        offsetVelocity += (meassurementPoint.position - lastPos) * springStrength * Time.fixedDeltaTime;

        lastLastPos = lastPos;

        lastPos += offsetVelocity * Time.fixedDeltaTime;
        if ((lastPos - meassurementPoint.position).magnitude > offsetMagnitude)
        {
            Vector3 diff = lastPos - meassurementPoint.position;
            float playerDiff = 0f;
            if (playerBody)
                playerDiff = Mathf.Abs(lastPlayerDiff - playerBody.velocity.magnitude);
            var accelerationDiff = Mathf.Abs(lastDiff - diff.magnitude);
            if (accelerationDiff - playerDiff * playermovementRelativeAdjustment >= shakeCorrectionReloadThreshold && Time.fixedTime - lastReload > reload_delay)
            {
                lastReload = Time.fixedTime;
                if (gunController && gunController.stats.Magazine != gunController.stats.Ammo)
                    slosh.Play(audioSource);

                gunController?.Reload(reload_ammount);
            }
            lastPos = meassurementPoint.position + diff.normalized * offsetMagnitude;
            lastDiff = diff.magnitude;
            lastPlayerDiff = playerDiff;
        }
        offsetTarget.position = lastPos;
    }
}
