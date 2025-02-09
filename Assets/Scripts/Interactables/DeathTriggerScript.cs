using UnityEngine;

public class DeathTriggerScript : MonoBehaviour
{
    [SerializeField]
    private bool isAIOnly;

    [SerializeField]
    private float damage = 1000;

    [SerializeField]
    private Vector3 knockbackDirection = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HitboxController>(out var hitbox) && hitbox.health.TryGetComponent<PlayerManager>(out var player))
        {
            if (isAIOnly && player is not AIManager)
                return;
            hitbox.DamageCollider(new DamageInfo(player, damage, hitbox.transform.position, knockbackDirection, DamageType.Falling));
        }
    }
}
