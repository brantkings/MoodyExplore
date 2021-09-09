using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Caster
{

    public class SphereCaster : Caster
    {
        [Header("Sphere")]
        [SerializeField]
        private float _radius = 0.5f;

        private Vector3 _lastOrigin;
        private Vector3 _lastDirectionTimesDistance;

        public override void DrawFormatGizmo(Vector3 center)
        {
            Gizmos.DrawWireSphere(center, _radius);
        }

        protected override Vector3 GetSpecificMinimumDistanceFromHit(in Vector3 hitPoint, in Vector3 hitNormal)
        {
            return hitNormal * _radius;
        }

        protected override bool MakeTheCast(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit)
        {
            _lastOrigin = origin;
            _lastDirectionTimesDistance = direction * distance;
            return Physics.SphereCast(origin, _radius, direction, out hit, distance, mask.value);
        }
    
        protected override int MakeTheCastAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance, RaycastHit[] results)
        {
            _lastOrigin = origin;
            _lastDirectionTimesDistance = direction * distance;
            return Physics.SphereCastNonAlloc(origin, _radius, direction, results, distance, mask.value, QueryTriggerInteraction.Ignore);
        }

        public override float GetOutsideDistance(Vector3 hitDirection)
        {
            return _radius;
        }
    }
}
