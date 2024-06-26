using UnityEngine;

/// <summary>
/// Modifier that hacks the affected player and fills their screen with spam.
/// </summary>
public class HackingModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField] private float damageToSpamAmount = 4;

    [SerializeField]
    private Priority priority = Priority.ARBITRARY;

    public Priority GetPriority()
    {
        return priority;
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.OnColliderHit += Hack;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= Hack;
    }

    private void Hack(RaycastHit hit, ref ProjectileState state)
    {
        if (!hit.collider.TryGetComponent<HitboxController>(out var hitboxController))
            return;
        if (!hitboxController.health.TryGetComponent<PlayerManager>(out var playerManager))
            return;
        // TODO: Actually disorient the AIs
        if (playerManager.identity.IsAI)
            return;
        if (playerManager.HUDController)
            playerManager.HUDController.PopupSpammer.Spam(Mathf.FloorToInt(state.damage / damageToSpamAmount));
    }
}
