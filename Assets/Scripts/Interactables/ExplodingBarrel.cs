using UnityEngine;

public class ExplodingBarrel : MonoBehaviour
{

    [SerializeField]
    private MeshRenderer barrelMesh;
    [SerializeField]
    private AudioSource audioSource;

    void Start()
    {
        GetComponent<HealthController>().onDeath += Explode;
    }

    private void Explode(HealthController controller, float damage, DamageInfo info)
    {
        barrelMesh.enabled = false;
        GetComponentInChildren<CapsuleCollider>().enabled = false;
        GetComponent<ExplosionController>().Explode(info.sourcePlayer);
        audioSource.Play();
        Destroy(gameObject, 4);
    }
}
