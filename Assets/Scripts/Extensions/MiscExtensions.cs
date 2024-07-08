using UnityEngine;

namespace MiscExtensions
{
    public static class MiscExtensions
    {
        public static Vector3 ClosestPoint(this RaycastHit hit, Vector3 position) =>
            hit.collider is BoxCollider or SphereCollider or CapsuleCollider or MeshCollider
            ? hit.collider.ClosestPoint(position)
            : hit.point;
    }
}
