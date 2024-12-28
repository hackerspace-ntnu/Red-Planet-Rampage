using UnityEngine;

public class FlippableSwitch : MonoBehaviour
{
    public delegate void FlipEvent(bool on);
    public FlipEvent OnFlip;

    [SerializeField]
    private HealthController health;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private bool isOn = false;

    private void Start()
    {
        health.onDamageTaken += OnDamageTaken;
    }

    private void OnDestroy()
    {
        health.onDamageTaken -= OnDamageTaken;
    }

    private void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        animator.SetTrigger("Flip");
        isOn = !isOn;
        OnFlip?.Invoke(isOn);
    }
}
