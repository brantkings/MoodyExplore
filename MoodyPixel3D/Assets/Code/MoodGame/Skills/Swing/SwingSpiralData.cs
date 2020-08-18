using UnityEngine;

namespace Code.MoodGame.Skills.Swing
{
    [CreateAssetMenu(fileName = "PUNCH_", menuName = "Long Hat House/Boxer/Punch Spiral Data", order = 0)]
    public class SwingSpiralData : SwingData
    {
        [Header("Spiral")]
        public float circleRadius = 2f;
        public float height = 1f;
        public Vector3 absoluteOffset = Vector3.zero;
        public Vector3 spiralUp = Vector3.up;
        public float circleTransposition = 0f;
        public float minArc = 0f, maxArc = 1f;
        public AnimationCurve movement = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


        protected Vector2 GetCirclePosition(float t)
        {
            t *= Mathf.PI * 2f;
            t += circleTransposition * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(t) * circleRadius, Mathf.Sin(t) * circleRadius);
        }

        protected Vector3 GetCirclePosition3D(float t)
        {
            Vector2 circle = GetCirclePosition(t);
            return new Vector3(circle.x, 0f, circle.y);
        }

        public override Vector3 GetLocalSwingPosition(float t)
        {
            t = movement.Evaluate(t);
            t = Mathf.Lerp(minArc, maxArc, t);
            //Quaternion rotateCircle = Quaternion.LookRotation(spiralUp, Vector3.up);
            Quaternion rotateCircle = Quaternion.FromToRotation(Vector3.up, spiralUp);
            return absoluteOffset + (spiralUp.normalized * height * t) + rotateCircle * GetCirclePosition3D(t);
        }
    }
}
