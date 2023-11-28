using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExplodingBarrel : MonoBehaviour
{
    [SerializeField] private MeshRenderer barrelMesh;

    [SerializeField] private DecalProjector explosionMark;

    [SerializeField] private LayerMask explosionMarkMask;

    [SerializeField] private float explosionMarkLifeTime = 60;

    private void Start()
    {
        GetComponent<HealthController>().onDeath += Explode;
    }

    private void Explode(HealthController controller, float damage, DamageInfo info)
    {
        barrelMesh.enabled = false;
        GetComponentInChildren<CapsuleCollider>().enabled = false;
        GetComponent<ExplosionController>().Explode(info.sourcePlayer);
        LeaveMark();
        Destroy(gameObject, 4);
    }

    private void LeaveMark()
    {
        if (!Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 100, explosionMarkMask))
            return;

        var position = hit.point + .5f * explosionMark.size.z * hit.normal;
        var spawnedDecal = Instantiate(explosionMark, position, Quaternion.LookRotation(-hit.normal));
        spawnedDecal.transform.parent = hit.collider.transform;

        Destroy(spawnedDecal, explosionMarkLifeTime);
    }
}
