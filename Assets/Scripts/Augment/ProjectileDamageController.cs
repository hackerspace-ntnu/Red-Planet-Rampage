using UnityEngine;

public enum DamageType
{
    Weapon,
    Explosion,
    Falling,
    Continuous,
    // TODO emit these two damage types (or do a separate effect type?)
    Fire,
    Hacking,
    // TODO determine if "knockback" should be its own type (effect type?)
}

public struct DamageInfo
{
    public PlayerManager sourcePlayer;

    public float damage;

    public float criticalHitMultiplier;

    public bool isCritical;

    public Vector3 position;

    public Vector3 force;

    public DamageType damageType;

    public DamageInfo(PlayerManager source, float damage, Vector3 position, Vector3 force, DamageType damageType, float criticalHitMultiplier = 1.2f)
    {
        this.sourcePlayer = source;
        this.damage = damage;
        this.criticalHitMultiplier = criticalHitMultiplier;
        this.isCritical = false;

        this.position = position;
        this.force = force;
        this.damageType = damageType;
    }

    public DamageInfo(PlayerManager source, NetworkDamageInfo info)
    {
        sourcePlayer = source;
        damage = info.damage;
        criticalHitMultiplier = info.criticalHitMultiplier;
        isCritical = info.isCritical;
        damageType = info.damageType;
        position = info.position;
        force = info.force;
    }

    public readonly string DeathToString(PlayerManager victim) =>
        sourcePlayer == victim
        ? $"{victim.identity.ToColorString()} {SuicideTypeToString()}"
        : $"{sourcePlayer.identity.ToColorString()} {KillTypeToString()} {victim.identity.ToColorString()}";

    private readonly string SuicideTypeToString() =>
        damageType switch
        {
            DamageType.Explosion => "exploded themselves",
            DamageType.Falling => "fell to their death",
            // TODO fix the emitted damage type from fire extension
            DamageType.Continuous => "burned in their own trail",
            _ => "committed sudoku"
        };

    private readonly string KillTypeToString() =>
        damageType switch
        {
            DamageType.Weapon => "shot",
            DamageType.Explosion => "blew up",
            // TODO fix the emitted damage type from fire extension
            DamageType.Continuous => "fried",
            DamageType.Hacking => "hacked",
            _ => "killed",
        };
}

public struct NetworkDamageInfo
{
    public uint sourcePlayer;

    public float damage;

    public float criticalHitMultiplier;

    public bool isCritical;

    public Vector3 position;

    public Vector3 force;

    public DamageType damageType;

    public NetworkDamageInfo(uint source, DamageInfo info)
    {
        sourcePlayer = source;
        damage = info.damage;
        criticalHitMultiplier = info.criticalHitMultiplier;
        isCritical = info.isCritical;
        damageType = info.damageType;
        position = info.position;
        force = info.force;
    }

    public NetworkDamageInfo(uint source, float damage, Vector3 position, Vector3 force, DamageType damageType, float criticalHitMultiplier = 1.2f)
    {
        this.sourcePlayer = source;
        this.damage = damage;
        this.criticalHitMultiplier = criticalHitMultiplier;
        this.isCritical = false;
        this.damageType = damageType;
        this.position = position;
        this.force = force;
    }
}

public class ProjectileDamageController : MonoBehaviour, ProjectileModifier
{
    public PlayerManager player;

    public void Attach(ProjectileController projectile)
    {
        player = projectile.player;
        projectile.OnHitboxCollision += DamageHitbox;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= DamageHitbox;
    }

    private void DamageHitbox(HitboxController controller, ref ProjectileState state)
    {
        var info = new DamageInfo(player, state.damage, state.position, state.direction, state.damageType, state.criticalHitMultiplier);
        if (controller.health && !state.hitHealthControllers.Contains(controller.health))
        {
            state.hitHealthControllers.Add(controller.health);
            controller.DamageCollider(info);
        }
    }
}
