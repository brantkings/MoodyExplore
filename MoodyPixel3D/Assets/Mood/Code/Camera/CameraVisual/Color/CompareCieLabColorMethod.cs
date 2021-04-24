using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colourful;

[CreateAssetMenu(menuName = "Long Hat House/Colors/Compare Method/Compare CIELab", fileName = "CompColor_CIELAB")]
public class CompareCieLabColorMethod : ColorComparerMethod, IColorComparerMethod<LabColor>
{
    Colourful.Conversion.ColourfulConverter converter = new Colourful.Conversion.ColourfulConverter();
    Colourful.Difference.IColorDifference<LabColor> dif = new Colourful.Difference.CIEDE2000ColorDifference();

    public LabColor Convert(in Color a)
    {
        return converter.ToLab(a.ToRGBColor());
    }

    public override double GetColorDistance(in Color a, in Color b)
    {
        converter.WhitePoint = Illuminants.D65; 
        RGBColor ca = a.ToRGBColor();
        RGBColor cb = b.ToRGBColor();

        LabColor la = converter.ToLab(ca);
        LabColor lb = converter.ToLab(cb);

        return GetColorDistance(la, lb);
    }

    public double GetColorDistance(in LabColor a, in LabColor b)
    {
        return dif.ComputeDifference(a, b);
    }
}
