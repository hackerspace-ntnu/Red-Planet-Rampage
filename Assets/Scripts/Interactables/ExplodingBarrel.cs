using System.Collections.Generic;
using System.Linq;
using CollectionExtensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class ExplodingBarrel : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] barrelMesh;

    [SerializeField] private ExplosionController explosion;

    [SerializeField] private DecalProjector explosionMark;

    [SerializeField] private LayerMask explosionMarkMask;

    [SerializeField] private float explosionMarkLifeTime = 60;

    [SerializeField] private VisualEffect[] dynamiteSmokes;

    private HealthController healthController;

    private bool isAlive = true;

    private static readonly List<ExplodingBarrel> barrels = new();

    public float Radius => explosion.Radius;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDeath += Explode;
        barrels.Add(this);
    }

    private void OnDestroy()
    {
        if (healthController) healthController.onDeath -= Explode;
        if (barrels.Contains(this)) barrels.Remove(this);
    }

    public static ExplodingBarrel GetViableExplodingBarrel(Vector3 from)
    {
        if (barrels.Count == 0)
            return null;

        return barrels
            .Select(barrel => new { barrel, distance = Vector3.Distance(from, barrel.transform.position) })
            .Where(pair => pair.distance < pair.barrel.explosion.Radius + 1)
            .MinBy(pair => pair.distance)
            ?.barrel;
    }


    private void Explode(HealthController controller, float damage, DamageInfo info)
    {
        if (!isAlive)
            return;
        barrelMesh.ToList().ForEach(b => b.enabled = false);
        GetComponentsInChildren<Collider>().ToList().ForEach(c => c.enabled = false);
        dynamiteSmokes.ToList().ForEach(smoke => smoke.enabled = false);
        explosion.Explode(info.sourcePlayer);
        LeaveMark();
        Destroy(gameObject, 4);
        isAlive = false;
        barrels.Remove(this);
    }

    private void LeaveMark()
    {
        // TODO This assumes that down is down on all maps. Refactor if gravity is made to work differently sometime.
        // The raycast should start approximately in the center of the barrel, thus the + transform.up
        if (!Physics.Raycast(transform.position + transform.up, Vector3.down, out var hit, explosion.Radius, explosionMarkMask))
            return;

        var position = hit.point + .5f * explosionMark.size.z * Vector3.up;
        var spawnedDecal = Instantiate(explosionMark, position, Quaternion.LookRotation(Vector3.down));
        spawnedDecal.transform.parent = hit.collider.transform;

        Destroy(spawnedDecal, explosionMarkLifeTime);
    }
}
