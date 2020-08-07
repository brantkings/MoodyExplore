using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{
    public static class LayerMaskUtils
    {
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask.value == (mask.value | (1 << layer));
        }
    }
}
