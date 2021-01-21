using System;
using Cinemachine.Utility;
using UnityEngine;

namespace Code.Animation.Humanoid
{
    public class WalkAnimation : MonoBehaviour
    {
        [SerializeField]
        private Animator _anim;

        
        public AnimatorID speedX = "SpeedX";
        public AnimatorID speedZ = "SpeedZ";
        public AnimatorID speedMultiplierWalk = "SpeedMultiplierWalk";
        public AnimatorID speedMultiplierRun = "SpeedMultiplierRun";

        public float speedAnimationWalk = 1f;
        public float speedAnimationRun = 1f;
        

        public void SetSpeed(Vector3 speed)
        {
            _anim.SetFloat(speedX, speed.x);
            _anim.SetFloat(speedZ, speed.y);
            float speedNum = speed.ProjectOntoPlane(Vector3.up).magnitude;
            _anim.SetFloat(speedMultiplierWalk, speedNum * speedAnimationWalk);
            _anim.SetFloat(speedMultiplierWalk, speedNum * speedAnimationRun);
        }
    }
}
