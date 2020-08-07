using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{

    public static class GizmosUtils
    {
        public static Color AI_Color = Color.magenta;
        public static Color InformativeColor = Color.cyan;
        public static Color SuccessColor = Color.green;
        public static Color FailureColor = Color.red;

        public static Color GetSuccessColor(bool isSuccess)
        {
            return isSuccess ? SuccessColor : FailureColor;
        }

        public static void DrawArrow(Vector3 from, Vector3 to)
        {
            Gizmos.DrawLine(from, to);
            Gizmos.DrawSphere(to, 0.1f);
            //Gizmos.matrix = GetMatrix(to, Quaternion.LookRotation(from - to));
            //Gizmos.DrawFrustum(to, 90f, 0.25f, 0f, 0.5f);
        }

        public static void DrawCone(Vector3 from, Vector3 direction, float angle, float length, int amount)
        {
            float rotAngleStep = 360f / amount;
            Vector3 toUpAngle = Quaternion.FromToRotation(Vector3.forward, Vector3.right) * direction;
            Vector3 firstConeRayDirection = Quaternion.AngleAxis(angle, toUpAngle) * direction;
            for (int i = 0; i < amount; i++)
            {
                Gizmos.DrawRay(from, Quaternion.AngleAxis(rotAngleStep * i, direction) * firstConeRayDirection);
            }
        }

        public static Matrix4x4 GetMatrix(Vector3 position, Quaternion rotation)
        {
            //return Matrix4x4.Rotate(rotation);
            return Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation);
        }
    }
}
