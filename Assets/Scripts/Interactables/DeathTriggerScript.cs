using UnityEngine;

public class DeathTriggerScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HitboxController>(out var hitbox)) 
        {
            hitbox.DamageCollider(new DamageInfo(hitbox.health.GetComponent<PlayerManager>(), 1000, hitbox.transform.position, Vector3.zero));
        }
    }
}
