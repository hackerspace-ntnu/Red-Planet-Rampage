using UnityEngine;

/// <summary>
/// Calculates damage based on the following speed range:
/// <list type="bullet">
/// <item>0: still</item>
/// <item>4: crouch</item>
/// <item>10: walk, dmg 40</item>
/// <item>11/12: leap, dmg 60</item>
/// <item>15: first bunnyleap, dmg 80</item>
/// <item>17: second bunnyleap</item>
/// <item>22+: falling velocity, dmg 100+</item>
/// <item>25: top speed</item>
/// </list>
/// </summary>
public class InheritMomentumModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField, Tooltip("Curve that determines damage modifier by velocity")]
    private AnimationCurve damageCurve;

    [SerializeField, Tooltip("The projectile speed will be *this* times faster")]
    private float speedMultiplier = 1.5f;

    [Range(0, 1)]
    [Tooltip("Projectile damage is this amount of total damage")]
    [SerializeField]
    private float projectileToAreaDamageRatio = .6f;

    [SerializeField]
    private Speedometer speedometer;

    private Rigidbody playerBody;
    private AIManager aiManager;

    private const string SpeedPropertyName = "PlayerSpeed";

    public void Attach(ProjectileController projectile)
    {
        if (!projectile.GunController.Player)
            return;

        playerBody = projectile.GunController.Player.GetComponent<Rigidbody>();
        aiManager = projectile.GunController.Player.GetComponent<AIManager>();
        speedometer.Body = playerBody;

        projectile.OnNetworkInit += OnNetworkInit;
        projectile.OnProjectileInit += OnProjectileInit;
    }

    public void Detach(ProjectileController projectile)
    {
        if (!projectile.GunController.Player)
            return;
        projectile.OnNetworkInit -= OnNetworkInit;
        projectile.OnProjectileInit -= OnProjectileInit;
    }

    private void OnNetworkInit(ref ProjectileFireData data, GunStats _)
    {
        var speed = playerBody.velocity.magnitude;
        if (playerBody.isKinematic && aiManager)
            speed = aiManager.Velocity.magnitude * 2f;

        data.additionalProperties.Add(new(SpeedPropertyName, speed));
    }

    private void OnProjectileInit(ref ProjectileState state, GunStats stats)
    {
        var speed = (float)state.additionalProperties[SpeedPropertyName];
        // Adjust gravity based on size so the enlargened projectiles feel bigger
        state.gravity *= stats.ProjectileScale;
        // Use player speed to determine dmg and speed
        var damage = state.damage * damageCurve.Evaluate(speed);
        state.damage = damage * projectileToAreaDamageRatio;
        state.additionalProperties[ExplosionModifier.AreaDamagePropertyName] = damage * (1 - projectileToAreaDamageRatio);
        state.speed = Mathf.Max(1f, speed * speedMultiplier);
    }
}
