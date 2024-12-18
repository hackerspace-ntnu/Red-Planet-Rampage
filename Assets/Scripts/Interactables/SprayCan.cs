using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

enum SprayCanState
{
    Inert,
    Spraying,
    Emptied,
}
public class SprayCan : MonoBehaviour
{
    [SerializeField]
    private float sprayForce = 1;

    [SerializeField]
    private Vector3 sprayDirectionOffset = new Vector3(.1f, 0f, -.2f);

    [SerializeField]
    private float timeBeforeSpraying = 0.1f;

    [SerializeField]
    private float timeSpentSpraying = 5;

    [SerializeField]
    private VisualEffect sprayEffect;

    private AudioSource audioSource;

    [SerializeField]
    private AudioGroup sprayNoise;
    
    [SerializeField]
    private DecalProjector sprayDecal;
    [SerializeField]
    private Color sprayColor;
    [SerializeField]
    private Material sprayMaterial;
    [SerializeField]
    private Material sprayCanMaterial;
    [SerializeField]
    private Renderer mesh;
    private Material instantiatedSprayMaterial;

    private HealthController healthController;
    private Rigidbody body;

    private SprayCanState state = SprayCanState.Inert;

    private Coroutine sprayRoutine;

    private ObjectPool<DecalProjector> sprayDecals;

    private float timeSinceDecalSpawn = 1;

    private const int allHitboxesAndGunsAndPlayersMask = (1 << 3) | (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody>();
        healthController = GetComponent<HealthController>();
        healthController.onDeath += OnDeath;
        sprayDecals = new ObjectPool<DecalProjector>(sprayDecal, 32);
        instantiatedSprayMaterial = Instantiate(sprayMaterial);
        instantiatedSprayMaterial.SetColor("_Color", sprayColor);
        mesh.material.color = new Color(sprayColor.r, sprayColor.g, sprayColor.b, 1f);
        sprayEffect.SetVector4("Color", new Vector4(sprayColor.r, sprayColor.g, sprayColor.b, 1f));
    }

    private void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        if (state is SprayCanState.Inert && sprayRoutine == null)
            sprayRoutine = StartCoroutine(StartSprayingEventually());
    }

    private IEnumerator StartSprayingEventually()
    {
        yield return new WaitForSeconds(timeBeforeSpraying);
        StartSpraying();
        yield return new WaitForSeconds(timeSpentSpraying);
        StopSpraying();
    }

    private void StartSpraying()
    {
        state = SprayCanState.Spraying;
        sprayEffect.SendEvent(VisualEffectAsset.PlayEventID);
        sprayEffect.SetBool("IsSpraying", true);
        audioSource.loop = true;
        sprayNoise.PlayExclusively(audioSource);
    }

    private void StopSpraying()
    {
        state = SprayCanState.Emptied;
        sprayEffect.SetBool("IsSpraying", false);
        audioSource.loop = false;
        audioSource.Stop();
    }

    private void FixedUpdate()
    {
        if (state is not SprayCanState.Spraying) return;

        var flyingDirection = (-transform.up + sprayDirectionOffset).normalized;
        body.AddForce(flyingDirection * sprayForce, ForceMode.VelocityChange);

        if (timeSinceDecalSpawn < 0.025)
        {
            timeSinceDecalSpawn += Time.deltaTime;
            return;
        }
        timeSinceDecalSpawn = 0;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit info, 1f))
        {

            // Avoid placing decals on players or their guns, as that leads to the heebie-jeebies
            if (((1 << info.collider.gameObject.layer) & allHitboxesAndGunsAndPlayersMask) > 0)
                return;
            // Specifically avoid players. This is still required, even with the layer check above.
            if (info.collider.TryGetComponent<HitboxController>(out var hitbox))
                if (hitbox.health && hitbox.health.TryGetComponent<PlayerManager>(out var _))
                    return;

            var decal = sprayDecals.Get();
            decal.transform.SetParent(info.transform);
            decal.transform.position = info.point - transform.forward;
            decal.transform.up = -transform.forward;
            decal.GetComponent<DecalProjector>().material = instantiatedSprayMaterial;
        }
            
            
    }

    private void OnDisable()
    {
        if (sprayDecals != null)
            sprayDecals.Flush();
    }
}
