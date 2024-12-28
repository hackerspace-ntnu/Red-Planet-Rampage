using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField]
    private float force = 30f;

    [SerializeField]
    private Vector3 direction = Vector3.forward;

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.TryGetComponent<Rigidbody>(out var rigidbody))
            return;

        var multiplier = ConveyorBeltDirector.DirectionMultiplier;
        var force = transform.TransformDirection(direction * multiplier) * this.force;
        rigidbody.AddForce(force, ForceMode.Acceleration);
    }
}
