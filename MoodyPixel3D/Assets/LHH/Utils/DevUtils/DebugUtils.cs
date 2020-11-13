using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{ 
    public static class DebugUtils
    {
        public static void DrawCircle(Vector3 position, float radius, Vector3 faceUp, Color color, float duration, bool depthTest = false, int divisions = 12)
        {
            if (divisions == 0) return;
            Quaternion upRot = Quaternion.FromToRotation(Vector3.up, faceUp);
            float angle = 360f / divisions;
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);
            Vector3 vec = new Vector3(0f, 0f, radius);
            Vector3 point = position + upRot * vec;
            while(divisions-- > 0)
            {
                vec = rot * vec;
                Vector3 nextPoint = position + upRot * vec;
                Debug.DrawLine(point, nextPoint, color, duration, depthTest);
                point = nextPoint;
            }
        }

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
