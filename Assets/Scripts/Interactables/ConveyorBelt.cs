using OperatorExtensions;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField]
    private float force = 30f;

    [SerializeField]
    private Vector3 direction = Vector3.forward;

    private const float visualSpeedFactor = 4f / 30f;
    private const float visualRotationSpeedFactor = 200f / 30f;

    [SerializeField]
    private Renderer beltMesh;

    [SerializeField]
    private int materialIndex;

    [SerializeField]
    private Transform[] wheels;

    private Material material;

    private float offset = 0;

    private void Start()
    {
        beltMesh.materials[materialIndex] = Instantiate(beltMesh.materials[materialIndex]);
        material = beltMesh.materials[materialIndex];
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.TryGetComponent<Rigidbody>(out var rigidbody))
            return;

        var multiplier = ConveyorBeltDirector.DirectionMultiplier;
        var force = transform.TransformDirection(direction * multiplier) * this.force;
        rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    private void Update()
    {
        // TODO more flexible than just z
        var multiplier = direction.z * ConveyorBeltDirector.DirectionMultiplier;
        material.SetFloat("_Direction", multiplier);
        offset += Time.deltaTime * visualSpeedFactor * force * multiplier;
        offset = offset.Mod(1);
        material.SetFloat("_Offset", offset);

        // Rotate wheeels
        foreach (var wheel in wheels)
        {
            var delta = Time.deltaTime * visualRotationSpeedFactor * force * multiplier;
            wheel.Rotate(new Vector3(delta, 0, 0));
        }
    }
}
