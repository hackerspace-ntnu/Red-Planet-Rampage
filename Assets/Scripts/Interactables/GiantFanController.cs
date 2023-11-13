using UnityEngine;

public class GiantFanController : MonoBehaviour
{
    [SerializeField]
    private float airForce = 50f;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbodyPlayer))
        {
            rigidbodyPlayer.AddForce(Vector3.up * airForce, ForceMode.Acceleration);
        }

    }
}
