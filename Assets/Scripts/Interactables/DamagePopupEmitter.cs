using UnityEngine;

public class DamagePopupEmitter : MonoBehaviour
{
    [SerializeField]
    private DamagePopup damagePopup;

    private HealthController healthController;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnHit;
    }

    private void Destroy()
    {
        healthController.onDamageTaken -= OnHit;
    }

    private void OnHit(HealthController healthController, float damage, DamageInfo info)
    {
        if (!info.sourcePlayer)
            return;
        var horizontalAxis = Vector3.Cross(info.force.normalized, Vector3.up);
        var position = info.position + Vector3.up * Random.Range(.3f, .5f) + horizontalAxis * Random.Range(-.2f, .2f);
        var popup = Instantiate(damagePopup, position, Quaternion.identity);

        // Real players can only see their own damage numbers, all players can see AI damage numbers
        // A player victim by AI damage cannot see damage done to them by AI popup (would obscure vision)
        bool isPlayer = info.sourcePlayer && info.sourcePlayer.inputManager;
        popup.gameObject.layer = isPlayer ?
            LayerMask.NameToLayer("Gun " + info.sourcePlayer.inputManager.playerInput.playerIndex) : gameObject.layer;

        popup.Camera = info.sourcePlayer.GunHolder.transform;
        popup.Damage = damage;
        popup.IsCritical = info.isCritical;
    }
}
