using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName ="PALETTE_COMPARING_Luminance_", menuName = "Long Hat House/Colors/Color Palette by Luminance")]
public class ComparingLuminanceColorPalette : Palette<ComparingLuminanceColorPalette.ColorsByLuminance>
{
    [System.Serializable]
    public struct ColorsByLuminance : IColorPalette<Color>
    {
        [Range(0f, 100f)]
        public float luminanceValue;
        public Color[] resultOrderOfLuminance;

        public IEnumerable<Color> GetColors()
        {
            return resultOrderOfLuminance;
        }
    }
}
