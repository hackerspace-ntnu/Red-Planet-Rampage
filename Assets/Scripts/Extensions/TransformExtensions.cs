using UnityEngine;

namespace TransformExtensions
{
    public static class TransformExtensions
    {
        public static void ScaleAndParent(this Transform transform, float scale, Transform parent)
        {
            transform.localScale = Vector3.one;
            transform.SetParent(parent);
            transform.localScale = new Vector3(scale / parent.lossyScale.x, scale / parent.lossyScale.y, scale / parent.lossyScale.z);
        }

        public static void ParentUnscaled(this Transform transform, Transform parent)
        {
            transform.localScale = Vector3.one;
            transform.SetParent(parent);
        }
    }
}
