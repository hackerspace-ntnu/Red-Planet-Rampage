using UnityEngine;

public class DeathTriggerScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HitboxController>(out HitboxController hitbox)) 
        {
            hitbox.DamageCollider(new DamageInfo(hitbox.health.GetComponent<PlayerManager>(), 99999));
        }
    }
}
