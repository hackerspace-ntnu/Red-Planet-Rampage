using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExplodingBarrel : MonoBehaviour
{
    [SerializeField] private MeshRenderer barrelMesh;

    [SerializeField] private ExplosionController explosion;

    [SerializeField] private DecalProjector explosionMark;

    [SerializeField] private LayerMask explosionMarkMask;

    [SerializeField] private float explosionMarkLifeTime = 60;

    private HealthController healthController;

    private bool isAlive = true;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDeath += Explode;
    }

    private void OnDestroy()
    {
        if (healthController) healthController.onDeath -= Explode;
    }


    private void Explode(HealthController controller, float damage, DamageInfo info)
    {
        if (!isAlive)
            return;
        barrelMesh.enabled = false;
        GetComponentsInChildren<Collider>().ToList().ForEach(c => c.enabled = false);
        explosion.Explode(info.sourcePlayer);
        LeaveMark();
        Destroy(gameObject, 4);
        isAlive = false;
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
