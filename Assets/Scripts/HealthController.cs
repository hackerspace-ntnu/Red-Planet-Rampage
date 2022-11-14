using UnityEngine;

public class HealthController : MonoBehaviour
{
    [SerializeField]
    private int maxhHealth = 100;
    public int MaxHealth{get => maxhHealth;}

    private int currentHealth = 0;

    public int CurrentHealth { get => currentHealth; }

    public delegate void damageEvent(HealthController healthController, int damage);
    public damageEvent OnDamageTaken;

    private void Awake()
    {
        foreach(var box in GetComponentsInChildren<HitboxController>())
        {
            box.health = this;
        }
    }

    private void Start()
    {
        currentHealth = maxhHealth;
    }

    public void dealDamage(int damage)
    {
        currentHealth -= damage;
        OnDamageTaken?.Invoke(this, damage);
    }
}
