using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{ 
    public static class DebugUtils
    {
        public static void DrawNormalStar(Vector3 position, float length, Quaternion rotation, Color color, float duration = 0.1f)
        {
            Debug.DrawRay(position, rotation * Vector3.up * length, color, duration);
            Debug.DrawRay(position, rotation * Vector3.right * length, color, duration);
            Debug.DrawRay(position, rotation * Vector3.forward * length, color, duration);
            Debug.DrawRay(position, rotation * Vector3.up * -length, color, duration);
            Debug.DrawRay(position, rotation * Vector3.right * -length, color, duration);
            Debug.DrawRay(position, rotation * Vector3.forward * -length, color, duration);
        }
    }
}
