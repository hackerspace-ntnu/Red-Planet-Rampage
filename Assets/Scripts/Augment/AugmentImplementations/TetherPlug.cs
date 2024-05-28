using UnityEngine;

public class TetherPlug : MonoBehaviour
{
    [SerializeField]
    private Transform plugOutput;
    public HealthController Health;
    public Transform WireOrigin => plugOutput;

}
