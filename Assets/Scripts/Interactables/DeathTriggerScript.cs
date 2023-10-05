using UnityEngine;

public class DeathTriggerScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HitboxController>(out HitboxController hitbox)) {
            //hitbox.health.onDeath?.Invoke(null, 9999, new DamageInfo(null, 99999));
            hitbox.DamageCollider(new DamageInfo(hitbox.health.GetComponent<PlayerManager>(), 99999));
        }
    }
}
