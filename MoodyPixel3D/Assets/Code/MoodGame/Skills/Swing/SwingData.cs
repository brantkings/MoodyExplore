using UnityEngine;

namespace Code.MoodGame.Skills.Swing
{
    public abstract class SwingData : ScriptableObject
    {
        [System.Serializable]
        public struct RigidbodyAffection
        {
            public float velocityCancelling;
            public float velocityReflection;

            public static RigidbodyAffection DefaultValue()
            {
                return new RigidbodyAffection()
                {
                    velocityCancelling = 1f,
                    velocityReflection = 0f
                };

            }
        }
        public float duration = 0.25f;
        public float afterTime = 0.05f;
        public float radius = 1f;
        public float capsuleHeight = 0f;
        public float impulseBase = 10f;
        public bool refreshAirHold = false;
        [SerializeField]
        private Vector3 baseDirection = Vector3.forward;
        [Range(0f, 1f)]
        public float amountOfHitDirection = 0f;
        public RigidbodyAffection rigidbodyData = RigidbodyAffection.DefaultValue();
        public KinematicBoost.Data puncherBoost;
        public KinematicBoost.Data onHitContraryBoost;
        public int amountOfTests = 1;

        public TimeManager.FrameFreezeData frameFreezeOnImpact;

        public abstract Vector3 GetLocalSwingPosition(float t);

        public Vector3 GetBaseDirection()
        {
            return baseDirection.normalized;
        }

        public Vector3 GetFinalDirection(Vector3 hitDirection, Transform puncher)
        {
            return Vector3.Slerp(puncher.TransformDirection(baseDirection), hitDirection, amountOfHitDirection);
        }
    }
}
