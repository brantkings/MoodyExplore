using System;
using UnityEngine;

namespace Code.Animation.Humanoid
{
    [RequireComponent(typeof(WalkAnimation))]
    public class WalkAnimationFromKinematicPlatformer : MonoBehaviour
    {
        private WalkAnimation _walk;

        public KinematicPlatformer platformer;
        
        private void Awake()
        {
            _walk = GetComponent<WalkAnimation>();
            
        }

        private void FixedUpdate()
        {
            _walk.SetSpeed(platformer.Velocity);
            _walk.SetGrounded(platformer.Grounded);
        }
    }
}
