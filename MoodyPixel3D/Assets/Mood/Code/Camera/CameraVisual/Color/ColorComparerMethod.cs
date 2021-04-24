using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IColorComparerMethod<T>
{
    double GetColorDistance(in T a, in T b);
}

public abstract class ColorComparerMethod : ScriptableObject, IColorComparerMethod<Color>
{
    public double GetDistance(in Colourful.IColorVector a, in Colourful.IColorVector b)
    {
        int len = Mathf.Min(a.Vector.Count, b.Vector.Count);
        double sum = 0f;
        for(int i = 0;i<len;i++)
        {
            double dist = a.Vector[i] - b.Vector[i];
            sum += dist * dist;
        }
        return System.Math.Sqrt(sum);
    }

    public abstract double GetColorDistance(in Color a, in Color b);
}

public abstract class PaletteComparerMethod<T> : ScriptableObject
{
    public abstract Color GetBestColor(in Color c, IColorPalette<T> palette);
}
