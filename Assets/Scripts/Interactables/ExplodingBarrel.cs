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

    private void OnDestroy()
    {
        GetComponent<HealthController>().onDeath -= Explode;
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
        // TODO This assumes that down is down on all maps. Refactor if gravity is made to work differently sometime.
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3, explosionMarkMask))
            return;
        
        var position = hit.point + .5f * explosionMark.size.z * Vector3.down;
        var spawnedDecal = Instantiate(explosionMark, position, Quaternion.LookRotation(Vector3.down));
        spawnedDecal.transform.parent = hit.collider.transform;

        Destroy(spawnedDecal, explosionMarkLifeTime);
    }
}
