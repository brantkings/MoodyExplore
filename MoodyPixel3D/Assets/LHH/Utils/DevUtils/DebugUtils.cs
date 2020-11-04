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

        public static void DrawArrow(Vector3 origin, Vector3 to, float wingLength, float wingAngle, Color color, float duration)
        {
            Debug.DrawLine(origin, to, color, duration);

            Vector3 ray = to - origin;
            float rayMag = ray.magnitude;
            wingLength = Mathf.Min(wingLength, rayMag * 0.5f);
            Quaternion angleR = Quaternion.Euler(0f, -wingAngle, wingAngle);
            Quaternion angleL = Quaternion.Euler(0f, wingAngle, -wingAngle);
            Vector3 wing = ray.normalized * wingLength;
            Vector3 wingR = angleR * wing;
            Vector3 wingL = angleL * wing;

            Debug.DrawLine(to, to - wingR, color, duration);
            Debug.DrawLine(to, to - wingL, color, duration);
        }
    }
}
