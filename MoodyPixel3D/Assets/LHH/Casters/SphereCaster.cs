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

        protected override void DrawGizmos()
        {
            Gizmos.DrawWireSphere(GetOriginPosition(), _radius);
            Gizmos.DrawLine(GetOriginPosition(), GetOriginPosition() + GetDefaultDirectionNormalized() * GetDefaultDistance());

            if(_lastDirectionTimesDistance != Vector3.zero)
            {
                Gizmos.DrawWireSphere(_lastOrigin + _lastDirectionTimesDistance, _radius);
                Gizmos.DrawWireSphere(_lastOrigin + _lastDirectionTimesDistance.normalized * -_safetyDistance, _radius);
            }
        }

        public override Vector3 GetCasterCenterOfHit(RaycastHit hit, float addedDistance)
        {
            return hit.point + hit.normal * (_radius + addedDistance);
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

        public override float GetOutsideDistance()
        {
            return _radius;
        }
    }
}
