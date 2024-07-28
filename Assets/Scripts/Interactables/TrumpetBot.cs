using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class TrumpetBot : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private HealthController healthController;

    private void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnHit;

    }

    private void OnDestroy()
    {
        healthController.onDamageTaken -= OnHit;
    }

    private void OnHit(HealthController healthController, float damage, DamageInfo info)
    {
        animator.SetTrigger("Hit");
        // TODO how to clear trigger??? it just works???
    }
}
