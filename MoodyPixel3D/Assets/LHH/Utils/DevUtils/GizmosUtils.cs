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

        public static void DrawArrow(Vector3 from, Vector3 to, float length)
        {
            Gizmos.DrawLine(from, to);
            //Gizmos.DrawSphere(to, 0.1f);
            DrawCone(to, (from - to).normalized, 30f, length, 4);
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
                Gizmos.DrawRay(from, (Quaternion.AngleAxis(rotAngleStep * i, direction) * firstConeRayDirection).normalized  * length);
            }
        }

        public static void DrawBox(Vector3 position, Vector3 halfExtents, Quaternion rotation)
        {
            IEnumerable<Vector3> rectangleGrounded = GetRectanglePoints(new Vector2(halfExtents.x, halfExtents.z), Vector3.up, Vector3.forward);

            Vector3? cycleNowBot = null, cycleNowTop = null;
            Vector3? cycleFirstBot = null, cycleFirstTop = null;
            foreach (Vector3 absVec in rectangleGrounded)
            {
                Vector3 vecBot = absVec - Vector3.up * halfExtents.y;
                Vector3 vecTop = absVec + Vector3.up * halfExtents.y;
                vecBot = position + rotation * vecBot;
                vecTop = position + rotation * vecTop;

                if (cycleNowBot.HasValue && cycleNowTop.HasValue)
                {
                    Gizmos.DrawLine(cycleNowBot.Value, vecBot);
                    Gizmos.DrawLine(cycleNowTop.Value, vecTop);
                }
                Gizmos.DrawLine(vecBot, vecTop);
                if (!cycleFirstBot.HasValue) cycleFirstBot = vecBot;
                if (!cycleFirstTop.HasValue) cycleFirstTop = vecTop;
                cycleNowBot = vecBot;
                cycleNowTop = vecTop;
            }
            Gizmos.DrawLine(cycleNowBot.Value, cycleFirstBot.Value); //Last line
            Gizmos.DrawLine(cycleNowTop.Value, cycleFirstTop.Value); 
        }

        public static void DrawCycle(params Vector3[] cycle)
        {
            DrawCycle(Vector3.zero, (IEnumerable<Vector3>)cycle);
        }

        public static void DrawMultiline(params Vector3[] line)
        {
            DrawMultiline(Vector3.zero, (IEnumerable<Vector3>)line);
        }

        public static void DrawCycle(Vector3 offset, IEnumerable<Vector3> cycle)
        {
            Vector3? now = null;
            Vector3? first = null;
            foreach (Vector3 absVec in cycle)
            {
                Vector3 vec = offset + absVec;
                if (now.HasValue)
                {
                    Gizmos.DrawLine(now.Value, vec);
                }
                if (!first.HasValue) first = vec;
                now = vec;
            }
            Gizmos.DrawLine(now.Value, first.Value); //Last line
        }

        public static void DrawMultiline(Vector3 offset, IEnumerable<Vector3> line)
        {
            Vector3? now = null;
            foreach (Vector3 absVec in line)
            {
                Vector3 vec = offset + absVec;
                if (now.HasValue)
                {
                    Gizmos.DrawLine(now.Value, vec);
                }
                now = vec;
            }
        }

        public static void DrawRectangle(Vector3 position, Vector2 halfExtents, Vector3 planeNormal, Vector3 inPlaneUp)
        {
            DrawCycle(position, GetRectanglePoints(halfExtents, planeNormal, inPlaneUp));
        }

        public static IEnumerable<Vector3> GetRectanglePoints(Vector2 halfExtents, Vector3 planeNormal, Vector3 inPlaneUp)
        {
            Vector3 inPlaneRight = Vector3.Cross(inPlaneUp, planeNormal);
            inPlaneUp.Normalize();
            inPlaneRight.Normalize();
            yield return halfExtents.x * inPlaneRight + halfExtents.y * inPlaneUp;
            yield return halfExtents.x * inPlaneRight - halfExtents.y * inPlaneUp;
            yield return -halfExtents.x * inPlaneRight - halfExtents.y * inPlaneUp;
            yield return -halfExtents.x * inPlaneRight + halfExtents.y * inPlaneUp;
        }

        public static Matrix4x4 GetMatrix(Vector3 position, Quaternion rotation)
        {
            //return Matrix4x4.Rotate(rotation);
            return Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation);
        }
    }
}
