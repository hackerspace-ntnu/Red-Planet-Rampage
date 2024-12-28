using UnityEngine;

public class GiantFanController : MonoBehaviour
{
    [SerializeField]
    private float airForce = 50f;

    [SerializeField]
    private Vector3 direction = Vector3.up;

    [SerializeField]
    private bool isRelative = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            var force = isRelative
                ? transform.TransformDirection(direction) * airForce
                : direction * airForce;
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }
    }
}
