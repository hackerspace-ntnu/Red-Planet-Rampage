using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperatorExtensions
{
    public static class Modulo
    {
        public static int Mod(this int a, int b)
        {
            int remainder = a % b;
            if (remainder >= 0)
                return remainder;
            if (b < 0)
                return remainder - b;
            // b > 0
            return remainder + b;
        }

        public static float Mod(this float a, float b)
        {
            float remainder = a % b;
            if (remainder >= 0)
                return remainder;
            if (b < 0)
                return remainder - b;
            // b > 0
            return remainder + b;
        }
    }
}

