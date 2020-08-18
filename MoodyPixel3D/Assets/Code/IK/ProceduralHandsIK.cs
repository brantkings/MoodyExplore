using System;
using UnityEngine;

namespace Code.IK
{
    public class ProceduralHandsIK : MonoBehaviour
    {
        public bool ikEnabled;
        public Transform rightHand;
        public Transform leftHand;

        public Transform rightHandStartPosition;
        public Transform leftHandStartPosition;


        public Animator anim;

        private void SetPosition(Transform from, Transform to)
        {
            if (from == null || to == null) return;
            to.position = from.position;
            to.rotation = from.rotation;
        }

        private void Start()
        {
            SetPosition(rightHandStartPosition, rightHand);
            SetPosition(leftHandStartPosition, leftHand);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            float ikWeight = ikEnabled ? 1f : 0f;
            SetIK(AvatarIKGoal.RightHand, rightHand, ikWeight);
            SetIK(AvatarIKGoal.LeftHand, leftHand, ikWeight);
        }

        private void SetIK(AvatarIKGoal goal, Transform target, float weight = 1f)
        {
            if (target == null) return;
            anim.SetIKPosition(goal, target.position);
            anim.SetIKPositionWeight(goal, weight);
            anim.SetIKRotation(goal, target.rotation);        
            anim.SetIKRotationWeight(goal, weight);
        }
    }
}
