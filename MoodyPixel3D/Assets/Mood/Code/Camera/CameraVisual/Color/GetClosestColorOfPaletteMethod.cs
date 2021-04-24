using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Long Hat House/Colors/Compare Palette/Compare Closest", fileName = "CompPalette_Closest_")]
public class GetClosestColorOfPaletteMethod : PaletteComparerMethod<Color>
{
    public ColorComparerMethod compareMethod;

    public override Color GetBestColor(in Color c, IColorPalette<Color> palette)
    {
        double best = double.MaxValue;
        Color bestColor = Color.black;

        foreach(Color other in palette.GetColors())
        {
            double distance = compareMethod.GetColorDistance(c, other);
            if (distance < best)
            {
                best = distance;
                bestColor = other;
            }
        }
        return bestColor;
    }

}
