using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorUtils 
{
    public static string ToHexStringRGB(this Color color)
    {
        return $"{ToHexString(color.r)}{ToHexString(color.g)}{ToHexString(color.b)}";
    }

    public static string ToHexStringRGBA(this Color color)
    {
        return $"{ToHexString(color.r)}{ToHexString(color.g)}{ToHexString(color.b)}{ToHexString(color.a)}";
    }

    public static string ToHexString(float value)
    {
        return Mathf.RoundToInt(value * 255).ToString("X2");
    }
}
