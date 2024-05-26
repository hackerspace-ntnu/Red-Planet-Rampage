using UnityEngine;

public class FloppyExtensionJiggleMesh : JiggleMesh
{
    [SerializeField]
    private float movementSensitivityWalking = 0.05f;

    [SerializeField]
    private float jiggleDistanceMultiplier = 1.5f;

    public PlayerManager player;
    private Vector2 normalizedPointer;
    public Vector2 NormalizedPointer => normalizedPointer;
    public Vector3 Direction { get; private set; }

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
        previousDiff += Vector3.up * 3;
    }

    private void UpdatePointer(Vector3 target)
    {
        var normalizedTarget = target.normalized;
        normalizedPointer = new Vector2(normalizedTarget.x, normalizedTarget.y);
    }

    private void Update()
    {
        Vector3 target = Vector3.Slerp(previousTarget, previousDiff, Time.deltaTime * elasticity);
        var distance = target;
        jiggleMaterial.SetVector("_Distance", target);
        var sensitivity = movementSensitivity;
        if (player && player.inputManager)
            sensitivity = player.inputManager.moveInput.magnitude < 0.2f ? movementSensitivity : movementSensitivityWalking;
        previousTarget = target + Quaternion.Inverse(transform.rotation) * -(oldPosition - transform.position) * sensitivity;
        previousDiff -= distance * jiggleFalloff;
        previousDiff /= 2;

        Direction = -(Quaternion.Inverse(transform.rotation) * (target - transform.forward) * jiggleDistanceMultiplier);
        UpdatePointer(Direction);
        oldPosition = transform.position;
    }
}
