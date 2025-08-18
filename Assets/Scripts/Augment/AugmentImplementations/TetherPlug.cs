using UnityEngine;

public class TetherPlug : MonoBehaviour
{
    [SerializeField]
    private Transform plugOutput;
    public HealthController Health;
    public Transform WireOrigin => plugOutput;
    [SerializeField]
    private Material plugMaterial;
    [SerializeField]
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer.materials[0] = Instantiate(meshRenderer.materials[0]);
        plugMaterial = meshRenderer.materials[0];
    }

    public void SetPulseStrength(float value)
    {
        plugMaterial.SetFloat("_PulseStrength", value);
    }
}
