using System;
using System.Diagnostics.CodeAnalysis;
using Colourful.Implementation;

namespace Colourful.Difference
{
    /// <summary>
    /// CIE Delta-E 2000 formula
    /// </summary>
    public sealed class CIEDE2000ColorDifference : IColorDifference<LabColor>
    {
        // parametric weighting factors:
        private const double k_H = 1;
        private const double k_L = 1;
        private const double k_C = 1;

        /// <param name="x">Reference color</param>
        /// <param name="y">Sample color</param>
        /// <remarks>Implemented according to: Sharma, Gaurav; Wencheng Wu, Edul N. Dalal (2005). "The CIEDE2000 color-difference formula: Implementation notes, supplementary test data, and mathematical observations" (http://www.ece.rochester.edu/~gsharma/ciede2000/ciede2000noteCRNA.pdf)</remarks>
        /// <returns>Delta-E (2000) color difference</returns>
        public double ComputeDifference(in LabColor x, in LabColor y)
        {
            if (x.WhitePoint != y.WhitePoint)
                throw new ArgumentException("Colors must have same white point to be compared.");

            // 1. Calculate C_prime, h_prime
            Calculate_a_prime(x.a, y.a, x.b, y.b, out double a_prime0, out double a_prime1);
            Calculate_C_prime(a_prime0, a_prime1, x.b, y.b, out double C_prime0, out double C_prime1);
            Calculate_h_prime(a_prime0, a_prime1, x.b, y.b, out double h_prime0, out double h_prime1);

            // 2. Calculate dL_prime, dC_prime, dH_prime
            double dL_prime = y.L - x.L; // eq. (8)
            double dC_prime = C_prime1 - C_prime0; // eq. (9)
            double dh_prime = Calculate_dh_prime(C_prime0, C_prime1, h_prime0, h_prime1);
            double dH_prime = 2 * Math.Sqrt(C_prime0 * C_prime1) * MathUtils.SinDeg(dh_prime / 2); // eq. (11)

            // 3. Calculate CIEDE2000 Color-Difference dE00
            double L_prime_mean = (x.L + y.L) / 2; // eq. (12)
            double C_prime_mean = (C_prime0 + C_prime1) / 2; // eq. (13)
            double h_prime_mean = Calculate_h_prime_mean(h_prime0, h_prime1, C_prime0, C_prime1);
            double T = 1 - 0.17 * MathUtils.CosDeg(h_prime_mean - 30) + 0.24 * MathUtils.CosDeg(2 * h_prime_mean)
                                                                   + 0.32 * MathUtils.CosDeg(3 * h_prime_mean + 6) - 0.20 * MathUtils.CosDeg(4 * h_prime_mean - 63); // eq. (15)
            double dTheta = 30 * Math.Exp(-MathUtils.Pow2((h_prime_mean - 275) / 25)); // eq. (16)
            double R_C = 2 * Math.Sqrt(MathUtils.Pow7(C_prime_mean) / (MathUtils.Pow7(C_prime_mean) + MathUtils.Pow7(25))); // eq. (17)
            double S_L = 1 + 0.015 * MathUtils.Pow2(L_prime_mean - 50) / Math.Sqrt(20 + MathUtils.Pow2(L_prime_mean - 50)); // eq. (18)
            double S_C = 1 + 0.045 * C_prime_mean; // eq. (19)
            double S_H = 1 + 0.015 * C_prime_mean * T; // eq. (20)
            double R_T = -MathUtils.SinDeg(2 * dTheta) * R_C; // eq. (21)

            double dE00 = Math.Sqrt(
                MathUtils.Pow2(dL_prime / (k_L * S_L)) +
                MathUtils.Pow2(dC_prime / (k_C * S_C)) +
                MathUtils.Pow2(dH_prime / (k_H * S_H)) +
                R_T * (dC_prime / (k_C * S_C)) * (dH_prime / (k_H * S_H))
            ); // eq. (22)

            return dE00;
        }

        private static void Calculate_a_prime(double a0, double a1, double b0, double b1, out double a_prime0, out double a_prime1)
        {
            double C_ab0 = Math.Sqrt(a0 * a0 + b0 * b0); // eq. (2)
            double C_ab1 = Math.Sqrt(a1 * a1 + b1 * b1);

            double C_ab_mean = (C_ab0 + C_ab1) / 2; // eq. (3)

            double G = 0.5d * (1 - Math.Sqrt(MathUtils.Pow7(C_ab_mean) / (MathUtils.Pow7(C_ab_mean) + MathUtils.Pow7(25)))); // eq. (4)

            a_prime0 = (1 + G) * a0; // eq. (5)
            a_prime1 = (1 + G) * a1;
        }

        private static void Calculate_C_prime(double a_prime0, double a_prime1, double b0, double b1, out double C_prime0, out double C_prime1)
        {
            C_prime0 = Math.Sqrt(a_prime0 * a_prime0 + b0 * b0); // eq. (6)
            C_prime1 = Math.Sqrt(a_prime1 * a_prime1 + b1 * b1);
        }

        private static void Calculate_h_prime(double a_prime0, double a_prime1, double b0, double b1, out double h_prime0, out double h_prime1)
        {
            // eq. (7)
            double hRadians = Math.Atan2(b0, a_prime0);
            double hDegrees = Angle.NormalizeDegree(Angle.RadianToDegree(hRadians));
            h_prime0 = hDegrees;

            hRadians = Math.Atan2(b1, a_prime1);
            hDegrees = Angle.NormalizeDegree(Angle.RadianToDegree(hRadians));
            h_prime1 = hDegrees;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static double Calculate_dh_prime(double C_prime0, double C_prime1, double h_prime0, double h_prime1)
        {
            // eq. (10)
            if (C_prime0 * C_prime1 == 0d)
                return 0;

            if (Math.Abs(h_prime1 - h_prime0) <= 180)
                return h_prime1 - h_prime0;

            if (h_prime1 - h_prime0 > 180)
                return h_prime1 - h_prime0 - 360;

            if (h_prime1 - h_prime0 < -180)
                return h_prime1 - h_prime0 + 360;

            return 0;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private static double Calculate_h_prime_mean(double h_prime0, double h_prime1, double C_prime0, double C_prime1)
        {
            // eq. (14)
            if (C_prime0 * C_prime1 == 0d)
                return h_prime0 + h_prime1;

            if (Math.Abs(h_prime0 - h_prime1) <= 180)
                return (h_prime0 + h_prime1) / 2;

            if (Math.Abs(h_prime0 - h_prime1) > 180 &&
                h_prime0 + h_prime1 < 360)
                return (h_prime0 + h_prime1 + 360) / 2;

            if (Math.Abs(h_prime0 - h_prime1) > 180 && h_prime0 + h_prime1 >= 360)
                return (h_prime0 + h_prime1 - 360) / 2;

            return h_prime0 + h_prime1;
        }
    }
}