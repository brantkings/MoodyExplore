using UnityEngine;

namespace Code.MoodGame.Skills.Swing
{
    [CreateAssetMenu(fileName = "PUNCH_", menuName = "Long Hat House/Boxer/Punch Arc Data", order = 0)]
    public class SwingArcData : SwingData
    {
        [Header("Arc")]
        public float distance = 2f;
        public float relativeToUpOffset = 1f;
        public Vector3 absoluteOffset = Vector3.zero;
        public Vector3 punchDirection = Vector3.forward;
        public Vector3 arcUp = Vector3.right;
        public float minArc = 0f, maxArc = 1f;
        public AnimationCurve movement = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


        protected Vector2 GetPunchPosition2D(float t)
        {
            t = movement.Evaluate(t);
            t = Mathf.Lerp(minArc, maxArc, t);
            t = (t - 0.5f) * 2f; // t = [-1,1]
            return new Vector2(t * 2f, -(t * t) + 1f); // (t, T^2) for that marvelous parabola
        }

        public override Vector3 GetLocalSwingPosition(float t)
        {
            Vector2 punchPos2D = GetPunchPosition2D(t);
            return absoluteOffset + (arcUp.normalized * relativeToUpOffset) + (punchPos2D.x * punchDirection + punchPos2D.y * arcUp);
        }
    }
}
