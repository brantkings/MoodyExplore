using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IColorPalette<T>
{
    IEnumerable<T> GetColors();
}

public class Palette<T> : ScriptableObject, IColorPalette<T>
{
    public T[] colors;
    public IEnumerable<T> GetColors()
    {
        return colors;
    }
}

[CreateAssetMenu(fileName ="PALETTE_", menuName = "Long Hat House/Colors/Color Palette")]
public class ColorPalette : Palette<Color>
{
    public virtual void SetMaterial(Material mat, string arrayID, string maxColorID)
    {
        mat.SetColorArray(arrayID, colors);
        mat.SetInt(maxColorID, colors.Length);
    }
}
