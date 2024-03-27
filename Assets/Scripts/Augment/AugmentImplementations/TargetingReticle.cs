using UnityEngine;

public class TargetingReticle : MonoBehaviour
{
    [SerializeField]
    private Renderer mesh;

    private void Start()
    {
        // TODO change color on suff
        mesh.transform.LeanRotateAroundLocal(Vector3.forward, 360, 1).setLoopClamp();

    }
}
