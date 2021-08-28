using System;
using UnityEngine;

namespace LHH.Caster
{
    
    public class CapsuleCaster : Caster
    {
        [SerializeField]
        private float _radius = 1f;
        [SerializeField]
        private float _length = 2f;
        [SerializeField]
        private Vector3 _capsuleDirection = Vector3.up;

        private Vector3 _lastOrigin;
        private Vector3 _lastDirectionTimesDistance;

        public override Vector3 GetCenterPositionOfHit(RaycastHit hit)
        {
            Vector3 centerOfSphere = hit.point + hit.normal * _radius;
            Vector3 sphereDistance = hit.point - centerOfSphere;

            return centerOfSphere + GetSpecificMinimumDistanceFromHit(hit.normal);
        }

        protected override Vector3 GetSpecificMinimumDistanceFromHit(Vector3 hitNormal)
        {
            float cosine = Vector3.Dot(hitNormal, _capsuleDirection);
            if (cosine > 0f) //Top part of the direction
            {
                return -_capsuleDirection * (_length * 0.5f);
            }
            else if (cosine < 0f) //Bottom part of the direction
            {
                return _capsuleDirection * (_length * 0.5f);
            }
            else //It is in the middle, in the cylinder
            {
                throw new NotImplementedException();
            }
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
