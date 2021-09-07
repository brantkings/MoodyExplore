using System;
using UnityEngine;

namespace LHH.Caster
{
    
    public class CapsuleCaster : Caster
    {
        [Header("Capsule")]
        [SerializeField]
        private float _radius = 1f;
        [SerializeField]
        private float _length = 2f;
        [SerializeField]
        private Vector3 _capsuleDirection = Vector3.up;

        private Vector3 _lastOrigin;
        private Vector3 _lastDirectionTimesDistance;

        protected override Vector3 GetSpecificMinimumDistanceFromHit(in Vector3 hitPoint, in Vector3 hitNormal)
        {
            Vector3 simpleSphereDistance = hitNormal.normalized * _radius;

            return simpleSphereDistance;

            /*float cosine = Vector3.Dot(hitNormal, _capsuleDirection);
            if (cosine > 0f) //Top part of the direction
            {
                return simpleSphereDistance - _capsuleDirection * (_length * 0.5f);
            }
            else if (cosine < 0f) //Bottom part of the direction
            {
                return simpleSphereDistance + _capsuleDirection * (_length * 0.5f);
            }
            else //It is in the middle, in the cylinder
            {
                return simpleSphereDistance;
            }*/
        }

        public override float GetOutsideDistance()
        {
            return _radius;
        }

        private Vector3 GetHalfLength()
        {
            return _capsuleDirection.normalized * (0.5f * _length);
        }

        protected override bool MakeTheCast(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit)
        {
            _lastOrigin = origin;
            _lastDirectionTimesDistance = direction * distance;
            Vector3 halfLength = GetHalfLength();
            Vector3 point1 = origin - halfLength;
            Vector3 point2 = origin + halfLength;
            return Physics.CapsuleCast(point1, point2, _radius, direction, out hit, distance, mask.value);
        }

        protected override int MakeTheCastAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance, RaycastHit[] results)
        {
            _lastOrigin = origin;
            _lastDirectionTimesDistance = direction * distance;
            Vector3 halfLength = GetHalfLength();
            Vector3 point1 = origin - halfLength;
            Vector3 point2 = origin + halfLength;
            return Physics.CapsuleCastNonAlloc(point1, point2, _radius, direction, results, distance, mask.value);
        }

        public override void DrawFormatGizmo(Vector3 position)
        {
            Vector3 halfLength = GetHalfLength();
            Gizmos.DrawWireSphere(position + halfLength, _radius);
            Gizmos.DrawWireSphere(position - halfLength, _radius);
        }

    }
}
