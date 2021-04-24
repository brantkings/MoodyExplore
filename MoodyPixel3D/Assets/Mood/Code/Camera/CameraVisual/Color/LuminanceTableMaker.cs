using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Long Hat House/Colors/Color Table Maker by Luminance", fileName = "COLOR_Table_ByLuminance")]
public class LuminanceTableMaker : ColorTableMaker<ComparingLuminanceColorPalette.ColorsByLuminance>
{

#if UNITY_EDITOR
    [ContextMenu("CreateTexture")]
    public void TestCreate()
    {
        CreateTexture();
    }
#endif
}
