using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Long Hat House/Colors/Compare Method/Compare RGB", fileName = "CompColor_RGB")]
public class CompareRGBColorMethod : ColorComparerMethod
{
    public override double GetColorDistance(in Color a, in Color b)
    {
        double distR = a.r - b.r;
        double distG = a.g - b.g;
        double distB = a.b - b.b;
        return System.Math.Sqrt(distR * distR + distG * distG + distB * distB);
    }
}
