using UnityEngine;

public class Rotation : MonoBehaviour
{
    [SerializeField]
    private Vector3 axis = Vector3.forward;

    [SerializeField]
    private float cycleDuration = 1;

    [SerializeField]
    private bool reverse = false;

    private void Start()
    {
        transform.LeanRotateAroundLocal(axis, reverse ? -360 : 360, cycleDuration).setLoopClamp();
    }
}
