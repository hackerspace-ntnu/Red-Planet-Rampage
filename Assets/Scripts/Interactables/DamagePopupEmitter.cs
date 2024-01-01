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
        var horizontalAxis = Vector3.Cross(info.force.normalized, Vector3.up);
        var position = info.position + Vector3.up * Random.Range(.3f, .5f) + horizontalAxis * Random.Range(-.2f, .2f);
        var popup = Instantiate(damagePopup, position, Quaternion.identity);

        popup.Camera = info.sourcePlayer.inputManager.transform;
        popup.Damage = damage;
    }
}
