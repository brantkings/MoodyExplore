using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName ="PALETTE_COMPARING", menuName = "Long Hat House/Colors/Color Palette that Compares")]
public class ComparingHueColorPalette : ScriptableObject
{
    int MAX_AMOUNT_COLORS = 256;

    [System.Serializable]
    public struct ComparisonStruct
    {
        public Color basicColor;
        public EachColorStruct[] resultOrderOfLuminance;
    }

    [System.Serializable]
    public struct EachColorStruct
    {
        public Color color;
        [Range(0f,100f)]
        public float luminance;
    }

    public ComparisonStruct gray;
    public ComparisonStruct[] colors;
    [Range(0f,1000f)]
    public float minDelta = 50f;
    [Range(0f, 100f)]
    public float minLuminanceToBlack = 0f;
    [Range(0f, 100f)]
    public float maxLuminanceToWhite = 100f;

    public void SetMaterial(Material mat)
    {
        GetResultColors(out List<Color> comparingColors, out int maxComparingColors, out List<Color> resultColors, out List<float> luminanceList, out List<float> indexesResults);
        mat.SetColorArray("_ComparingColors", comparingColors);
        mat.SetInt("_MaxColors", maxComparingColors);
        mat.SetColorArray("_Colors", resultColors);
        mat.SetFloatArray("_Luminances", luminanceList);
        mat.SetFloatArray("_ColorIndexes", indexesResults);
        mat.SetFloat("_MinDelta", minDelta);
        mat.SetFloat("_MinLuminance", minLuminanceToBlack);
        mat.SetFloat("_MaxLuminance", maxLuminanceToWhite);
    }

    [ContextMenu("Spread luminance")]
    public void SpreadLuminance()
    {
        float maxLuminance = 100f;
        foreach(ComparisonStruct s in AllStructs())
        {
            int len = s.resultOrderOfLuminance.Length;
            if(len > 1)
            {
                float step = maxLuminance / (len - 1);
                for (int i = 0; i < len; i++) 
                {
                    s.resultOrderOfLuminance[i].luminance = step * i;
                }
            }
        }
    }

    public void SetMaterial(Material mat, string arrayID, string maxColorID)
    {
        List<Color> colorList = AllResultColors().ToList();
        mat.SetColorArray(arrayID, colorList);
        mat.SetInt(maxColorID, colorList.Count);
    }

    private int AmountOfStructs()
    {
        return 1 + colors.Length; //Includes gray
    }

    private IEnumerable<ComparisonStruct> AllStructs()
    {
        yield return gray;
        foreach (var c in colors) yield return c;
    }

    private IEnumerable<Color> ComparingColors()
    {
        foreach (ComparisonStruct color in AllStructs()) yield return color.basicColor;
    }

    private IEnumerable<EachColorStruct> AllResults()
    {
        foreach (ComparisonStruct color in AllStructs()) 
            foreach(EachColorStruct c in color.resultOrderOfLuminance) yield return c;
    }

    private IEnumerable<Color> AllResultColors()
    {
        foreach (EachColorStruct c in AllResults()) yield return c.color;
    }

    private void GetResultColors(out List<Color> comparingColors, out int maxComparingColors, out List<Color> colors, out List<float> luminances, out List<float> indexes)
    {
        int len = AmountOfStructs();
        colors = new List<Color>(MAX_AMOUNT_COLORS);
        luminances = new List<float>(MAX_AMOUNT_COLORS);
        comparingColors = new List<Color>(len);
        indexes = new List<float>(len + 1);
        maxComparingColors = len;
        int i = 0;
        foreach (ComparisonStruct color in AllStructs())
        {
            indexes.Add(i);
            comparingColors.Add(color.basicColor);
            for (int j = 0, lenC = color.resultOrderOfLuminance.Length; j < lenC && i<MAX_AMOUNT_COLORS; j++, i++) 
            {
                colors.Add(color.resultOrderOfLuminance[j].color);
                luminances.Add(color.resultOrderOfLuminance[j].luminance);
            }
        }
        indexes.Add(i); //Need where to finish
    }
}
