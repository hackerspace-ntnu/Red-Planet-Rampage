using Mirror;
using UnityEngine;

public class HealthController : NetworkBehaviour
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

    public PlayerManager Player { get; private set; }

    private void Awake()
    {
        foreach (var hitbox in GetComponentsInChildren<HitboxController>())
        {
            hitbox.health = this;
        }
    }

    private void Start()
    {
        currentHealth = maxhHealth;
        Player = GetComponent<PlayerManager>();
    }

    public void DealDamage(DamageInfo info)
    {
        if (!isNetworked)
        {
            ActuallyDealDamage(info);
        }
        else if (isServer)
        {
            Debug.Log($"Dealing {info.damage} from {info.sourcePlayer.identity.playerName} on server");
            var networkInfo = new NetworkDamageInfo(info.sourcePlayer.id, info);
            DealDamageRpc(networkInfo);
        }
    }

    [ClientRpc]
    private void DealDamageRpc(NetworkDamageInfo networkInfo)
    {
        var source = MatchController.Singleton ? MatchController.Singleton.PlayerById[networkInfo.sourcePlayer] : null;
        var info = new DamageInfo(source, networkInfo);
        ActuallyDealDamage(info);
    }

    private void ActuallyDealDamage(DamageInfo info)
    {
        currentHealth -= info.damage;
        onDamageTaken?.Invoke(this, info.damage, info);
        if (currentHealth <= 0)
        {
            onDeath?.Invoke(this, info.damage, info);
        }
    }
}
