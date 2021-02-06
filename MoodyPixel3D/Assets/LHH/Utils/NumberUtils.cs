using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{
    public static class NumberUtils
    {
        public enum Comparer
        {
            Equals,
            Different,
            Greater,
            GreaterOrEqual,
            Lesser,
            LesserOrEqual,
        }

        [System.Serializable]
        public struct NumberComparer<T>
        {
            public T number;
            public Comparer comparer;

            public bool Compare(T with)
            {
                float c = Comparer<T>.Default.Compare(with, number);
                Debug.LogFormat("Height checker {0} {3} {1} = {2}", with, number, c, comparer);

                switch (comparer)
                {
                    case Comparer.Equals:
                        return c == 0;
                    case Comparer.Different:
                        return c != 0;
                    case Comparer.Greater:
                        return c > 0;
                    case Comparer.GreaterOrEqual:
                        return c >= 0;
                    case Comparer.Lesser:
                        return c < 0;
                    case Comparer.LesserOrEqual:
                        return c <= 0;
                    default:
                        return false;
                }
            }
        }


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
