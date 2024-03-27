using System.Collections.Generic;
using UnityEngine;

public class FallingHazard : MonoBehaviour
{
    [SerializeField] private float deadlyVelocityThreshold = 50;
    [SerializeField] private float damage = 50;

    private Rigidbody body;
    private PlayerManager player;

    public PlayerManager Player
    {
        get => player;
        set => player = value;
    }

    private HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.AddForce(300f * Vector3.down, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        // Squared magnitude performs better cuz no square root is required :)
        var isMovingFastEnoughToKill = body.velocity.sqrMagnitude > deadlyVelocityThreshold;
        if (!isMovingFastEnoughToKill)
            return;

        foreach (var contact in other.contacts)
        {
            DealDamage(contact);
        }
    }

    private void DealDamage(ContactPoint contact)
    {
        var damageInfo = new DamageInfo
        {
            damage = damage,
            damageType = DamageType.Weapon,
            position = contact.point,
            force = damage * body.velocity,
            sourcePlayer = player
        };

        HealthController healthController = null;

        if (contact.otherCollider.TryGetComponent<HitboxController>(out var hitbox))
        {
            healthController = hitbox.health;
        }
        else if (contact.otherCollider.TryGetComponent<HealthController>(out var health))
        {
            healthController = health;
        }

        if (!healthController || hitHealthControllers.Contains(healthController))
            return;

        hitHealthControllers.Add(healthController);
        healthController.DealDamage(damageInfo);
    }
}