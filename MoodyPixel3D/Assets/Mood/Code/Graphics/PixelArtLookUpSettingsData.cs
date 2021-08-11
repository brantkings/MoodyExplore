using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pixel Art Camera/Settings Data Pointer", fileName = "PA_EffectPointer_")]
public class PixelArtLookUpSettingsData : ScriptableObject
{
    public Color addColor = Color.black;
    public Color multiplyColor = Color.white;
}