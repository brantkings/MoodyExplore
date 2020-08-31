using DG.Tweening;
using UnityEngine;

namespace Code.Feedback
{
    public class PunchScaleOnEnable : TweenOnEnable<Transform, Vector3>
    {
        public Vector3 punchForce;
        public Vector3 rotationForce;
        public int vibratoPunch = 10;
        public float elasticityPunch = 1f;
        public int vibratoRotation = 10;
        public float elasticityRotation = 1f;
        
        protected override Tween ExecuteTweenItself(Vector3 to, float duration)
        {
            Sequence seq = DOTween.Sequence();
            seq.Insert(0f, Addon.DOPunchScale(punchForce, duration, vibratoPunch, elasticityPunch));
            seq.Insert(0f, Addon.DOPunchRotation(rotationForce, duration, vibratoRotation, elasticityRotation));
            return seq;
        }

        protected override Vector3 GetInValue()
        {
            return Vector3.one;
        }

        protected override Vector3 GetOutValue()
        {
            return Vector3.one;
        }

        public override void SetValue(Vector3 value)
        {
            Addon.localScale = value;
        }
    }
}
