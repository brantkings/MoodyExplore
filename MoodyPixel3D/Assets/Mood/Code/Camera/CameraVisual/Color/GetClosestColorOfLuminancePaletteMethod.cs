using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colourful;


[CreateAssetMenu(fileName = "PALETTE_COMPARING_Luminance_", menuName = "Long Hat House/Colors/Compare Palette/Compare Color Palette by Luminance")]
public class GetClosestColorOfLuminancePaletteMethod : PaletteComparerMethod<ComparingLuminanceColorPalette.ColorsByLuminance>
{
    public ColorComparerMethod compareWhenInSameLuminanceMethod;

    Colourful.Conversion.ColourfulConverter converter = new Colourful.Conversion.ColourfulConverter();
    public override Color GetBestColor(in Color c, IColorPalette<ComparingLuminanceColorPalette.ColorsByLuminance> palette)
    {
        LabColor lab = new Colourful.Conversion.ColourfulConverter().ToLab(c.ToRGBColor());

        float bestDistance = float.MaxValue;
        ComparingLuminanceColorPalette.ColorsByLuminance? bestSet = null;
        foreach(var str in palette.GetColors())
        {
            float distance = Mathf.Abs((float)lab.L - str.luminanceValue);
            if(distance < bestDistance)
            {
                bestDistance = distance;
                bestSet = str;
            }
        }
        if (bestSet.HasValue) return GetBestColorWithin(in lab, in c, bestSet.Value);
        else return Color.magenta; //Bug
    }

    private Color GetBestColorWithin(in LabColor c, in Color original, ComparingLuminanceColorPalette.ColorsByLuminance palette)
    {
        double best = double.MaxValue;
        Color bestColor = Color.black;

        IColorComparerMethod<LabColor> labCompare = compareWhenInSameLuminanceMethod as IColorComparerMethod<LabColor>;

        foreach (Color other in palette.GetColors())
        {
            double distance = labCompare != null ? labCompare.GetColorDistance(c, converter.ToLab(other.ToRGBColor())) : compareWhenInSameLuminanceMethod.GetColorDistance(original, other);
            if (distance < best)
            {
                best = distance;
                bestColor = other;
            }
        }
        return bestColor;
    }
}
