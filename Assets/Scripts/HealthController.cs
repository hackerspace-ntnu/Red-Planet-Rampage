using UnityEngine;

public class HealthController : MonoBehaviour
{
    [SerializeField]
    private int maxhHealth = 100;
    public int MaxHealth { get => maxhHealth; }
    [SerializeField]
    private float currentHealth = 0;

    public float CurrentHealth { get => currentHealth; }

    public delegate void DamageEvent(HealthController healthController, float damage, DamageInfo info);
    public DamageEvent onDamageTaken;

    public DamageEvent onDeath;

    private void Awake()
    {
        foreach (var box in GetComponentsInChildren<HitboxController>())
        {
            box.health = this;
        }
    }

    private void Start()
    {
        currentHealth = maxhHealth;
    }

    public void dealDamage(DamageInfo info)
    {
        
        currentHealth -= info.damage;
        onDamageTaken?.Invoke(this, info.damage, info);
        if (currentHealth <= 0)
        {
            onDeath?.Invoke(this, info.damage, info);
        }
    }
}
