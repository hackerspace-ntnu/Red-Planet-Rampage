using UnityEngine;

public class ExplodingBarrel : MonoBehaviour
{

    [SerializeField]
    private MeshRenderer barrelMesh;

    void Start()
    {
        GetComponent<HealthController>().onDeath += Explode;
    }

    private void Explode(HealthController controller, float damage, DamageInfo info)
    {
        barrelMesh.enabled = false;
        GetComponentInChildren<CapsuleCollider>().enabled = false;
        GetComponent<ExplosionController>().Explode(info.sourcePlayer);
        Destroy(gameObject, 4);
    }
}
