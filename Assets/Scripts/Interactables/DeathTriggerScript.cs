using UnityEngine;

public class DeathTriggerScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HitboxController>(out var hitbox) && hitbox.health.TryGetComponent<PlayerManager>(out var player))
        {
            hitbox.DamageCollider(new DamageInfo(player, 1000, hitbox.transform.position, Vector3.zero, DamageType.Falling));
        }
    }
}
