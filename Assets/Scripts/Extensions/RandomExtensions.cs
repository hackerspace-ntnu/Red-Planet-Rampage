using UnityEngine;
using Random = System.Random;

namespace RandomExtensions
{
    public static class RandomExtensions
    {
        public static float NextSingle(this Random random) => (float)random.NextDouble();

        public static float Range(this Random random, float min, float max) => min + (max - min) * random.NextSingle();
        public static int Range(this Random random, int min, int max) => random.Next(min, max);

        public static Quaternion Rotation(this Random random)
        {
            float u = random.NextSingle();
            float v = random.NextSingle();
            float w = random.NextSingle();

            float oneMinusUSqrt = Mathf.Sqrt(1 - u);
            float uSqrt = Mathf.Sqrt(u);
            float tauV = 2 * Mathf.PI * v;
            float tauW = 2 * Mathf.PI * w;
            // Using formula for uniform quaternion
            return new Quaternion(oneMinusUSqrt * Mathf.Sin(tauV), oneMinusUSqrt * Mathf.Cos(tauV), uSqrt * Mathf.Sin(tauW), uSqrt * Mathf.Sin(tauV));
        }

        public static Vector3 UnitVector(this Random random)
        {
            return new Vector3(2 * random.NextSingle() - 1, 2 * random.NextSingle() - 1, 2 * random.NextSingle() - 1).normalized;
        }
    }
}
