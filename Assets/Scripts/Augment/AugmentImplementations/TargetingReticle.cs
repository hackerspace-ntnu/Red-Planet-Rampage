using UnityEngine;

public class TargetingReticle : MonoBehaviour
{
    [SerializeField]
    private Renderer mesh;

    private void Start()
    {
        mesh.transform.LeanRotateAroundLocal(Vector3.forward, 360, 1).setLoopClamp();
    }
}
