using UnityEngine;

public class FloppyExtensionJiggleMesh : JiggleMesh
{
    [SerializeField]
    private float movementSensitivityWalking = 0.05f;

    public PlayerManager player;

    private void Start()
    {
        // Intantitate JiggleMaterial
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[jiggleMaterialIndex] = Instantiate(meshRenderer.materials[jiggleMaterialIndex]);
        jiggleMaterial = meshRenderer.materials[jiggleMaterialIndex];
        // Set initial values
        oldPosition = transform.position;
    }

    public void AnimatePushback()
    {
        previousDiff += Vector3.up*3;
    }

    private void Update()
    {
        Vector3 target = Vector3.Slerp(previousTarget, previousDiff - jiggleForwardDirection, Time.deltaTime * elasticity);
        var distance = target - jiggleForwardDirection;
        jiggleMaterial.SetVector("_Distance", target);
        var sensitivity = player.inputManager.moveInput.magnitude < 0.2f ? movementSensitivity : movementSensitivityWalking;
        previousTarget = target + (oldPosition - transform.position) * sensitivity;
        previousDiff -= distance * 0.90f;
        previousDiff /= 2;
        oldPosition = transform.position;
    }
}
