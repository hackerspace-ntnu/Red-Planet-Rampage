using UnityEngine;

public class GiantFanController : MonoBehaviour
{
    private CapsuleCollider capsuleCollider;

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rigidbodyPlayer))
        {
            rigidbodyPlayer = other.gameObject.GetComponent<Rigidbody>();
            rigidbodyPlayer.AddForce(Vector3.up * 35f, ForceMode.Acceleration);
        }

    }
}
