using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colourful
{
    public static class ColourfulToUnity
    {
        public static RGBColor ToRGBColor(this Color c, IRGBWorkingSpace space)
        {
            return new RGBColor(c.r, c.g, c.b, space);
        }

        public static RGBColor ToRGBColor(this Color c)
        {
            return new RGBColor(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b), RGBWorkingSpaces.sRGB);
        }


        public static Color ToColor(this RGBColor c)
        {
            return new Color((float)c.R, (float)c.G, (float)c.B);
        }
    }

}
