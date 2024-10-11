using UnityEngine;

namespace VectorExtensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Constrain aiming angle vertically and wrap horizontally.
        /// + and - Mathf.Deg2Rad is offsetting with 1 degree in radians,
        /// which is neccessary to avoid IK shortest path slerping that causes animations to break at exactly the halfway points.
        /// This is way more computationally efficient than creating edgecase checks in IK with practically no gameplay impact
        /// </summary>
        /// <param name="angles"></param>
        public static Vector2 ClampedLookAngles(this Vector2 angles)
        {
            var y = Mathf.Clamp(angles.y, -Mathf.PI / 2 + Mathf.Deg2Rad, Mathf.PI / 2 - Mathf.Deg2Rad);
            var x = (angles.x + Mathf.PI) % (2 * Mathf.PI) - Mathf.PI;
            return new Vector2(x, y);
        }

        public static float MaxAbsComponent(this Vector2 vector) =>
            Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));

        public static Vector2 xz(this Vector3 vector) =>
            new Vector2(vector.x, vector.z);
    }
}
