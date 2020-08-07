using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="PALETTE_", menuName = "Long Hat House/Color Palette")]
public class ColorPalette : ScriptableObject
{
    public Color[] colors;

    public void SetMaterial(Material mat, string arrayID, string maxColorID)
    {
        mat.SetColorArray(arrayID, colors);
        mat.SetInt(maxColorID, colors.Length);
    }
}
