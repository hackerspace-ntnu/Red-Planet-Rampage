using Mirror;
using UnityEngine;
using UnityEngine.VFX;

public class TetherBody : GunBody
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
    private float pullForce = 10f;
    [SerializeField]
    private float ropeLength = 60f;

    private bool isWired = true;
    private float oldLength = 0f;

    private SpawnDelegate onSpawnPlug;
    private UnSpawnDelegate onUnSpawnPlug;

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

        var assetId = NetworkManager.singleton.spawnPrefabs[0].GetComponent<NetworkIdentity>().assetId;
        if (isServer)
        {

            plugAnchor = Instantiate(plugAnchorPrefab);
            NetworkServer.Spawn(plugAnchor.gameObject);
            SpawnPlug();
        }
        else
        {
            NetworkClient.RegisterSpawnHandler(assetId, onSpawnPlug, onUnSpawnPlug);
            onSpawnPlug += SpawnPlug;
            onUnSpawnPlug += UnSpawnPlug;
        }

        playerBody = gunController.Player.GetComponent<Rigidbody>();
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandLeft.gameObject.SetActive(true);
        gunController.onFireNoAmmo += CheckForWirePlanting;
        movement = gunController.Player.GetComponent<PlayerMovement>();
    }

    private GameObject SpawnPlug(Vector3 position, uint assetId)
    {
        return SpawnPlug();
    }

    private GameObject SpawnPlug()
    {
        plugAnchor = Instantiate(plugAnchorPrefab);
        plugAnchor.Health.onDeath += RemoveRope;
        plugAnchor.transform.position = gunController.Player.transform.position;
        rope.Anchor = plugAnchor.transform;
        rope.ResetRope(plugAnchor.WireOrigin);
        return plugAnchor.gameObject;
    }

    private void UnSpawnPlug(GameObject spawned)
    {
        plugAnchor.Health.onDeath -= RemoveRope;
        Destroy(plugAnchor);
    }


    private void OnDestroy()
    {
        onSpawnPlug -= SpawnPlug;
        onUnSpawnPlug -= UnSpawnPlug;
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
        if (isWired || movement.StateIsAir)
            return;
        // TODO network this!
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
        rope.enabled = true;
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

}
