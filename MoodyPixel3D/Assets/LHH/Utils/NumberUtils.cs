using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{
    public static class NumberUtils
    {
        /// <summary>
        /// Get the max between the absolutes of a and b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MaxAbs(float a, float b)
        {
            if (Mathf.Abs(a) > Mathf.Abs(b)) return a;
            else return b;
        }

        /// <summary>
        /// Get the min between the absolutes of a and b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MinAbs(float a, float b)
        {
            if (Mathf.Abs(a) < Mathf.Abs(b)) return a;
            else return b;
        }

        /// <summary>
        /// Get the max between the absolutes of the parameters.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MaxAbs(params float[] nums)
        {
            float max = 0f;
            foreach (float num in nums) max = NumberUtils.MaxAbs(num, max);
            return max;
        }

        /// <summary>
        /// Get the min between the absolutes of the parameters.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float MinAbs(params float[] nums)
        {
            float max = float.NegativeInfinity;
            foreach (float num in nums) max = NumberUtils.MinAbs(num, max);
            return max;
        }
    }
}
