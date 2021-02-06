using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Utils;

namespace LHH.Caster
{

    public abstract class Caster : MonoBehaviour
    {
        [SerializeField]
        private LayerMask _hitMask;
        [SerializeField]
        private LayerMask _obstacleMask;
        [SerializeField]
        private Transform _defaultOrigin;
        [SerializeField]
        private Vector3 _defaultOriginOffset = Vector3.zero;
        [SerializeField]
        private Vector3 _defaultDirectionRelative = -Vector3.up;
        [SerializeField]
        private float _distance = 1f;
        [SerializeField]
        private int _maxSimultaneousHits = 5;

        [Header("Offsets and distances")]
        [Tooltip("An amount of distance added moving the origin backwards so give leeway to hit a collider right in front.")]
        [SerializeField]
        protected float _safetyDistance = 0f;
        [Tooltip("Move the origin in an offset forward to simulate it actually beginning in an sphere around the origin.")]
        [SerializeField]
        protected float _originDistanceOffset = 0f;
        [Tooltip("Move the origin normalized means that it will always have the exact length of the number above.")]
        [SerializeField]
        protected bool _normalizeOriginDistanceOffset = true;
        [Tooltip("Restrict the origin distance offset to a plane dictated by a vector, relative to the caster's transform. If the plane is 0,0,0 nothing will happen.")]
        [SerializeField]
        protected Vector3 _restrictOriginDistanceOffsetToPlane;

        private RaycastHit[] _hitsAllArray;

        protected RaycastHit[] HitsCache
        {
            get
            {
                if (_hitsAllArray == null) _hitsAllArray = new RaycastHit[_maxSimultaneousHits];
                return _hitsAllArray;
            }
        }

        protected virtual LayerMask LayerMask
        {
            get
            {
                return _hitMask | _obstacleMask;
            }
        }

        public LayerMask HitMask
        {
            get
            {
                return _hitMask;
            }
        }

        public LayerMask ObstacleMask
        {
            get
            {
                return _obstacleMask;
            }
        }

        protected Transform Origin
        {
            get
            {
                if (_defaultOrigin == null) _defaultOrigin = transform;
                return _defaultOrigin;
            }
        }

        public float SafetyDistance
        {
            get
            {
                return _safetyDistance;
            }
        }

        public virtual Vector3 GetOriginPosition()
        {
            return Origin.TransformPoint(_defaultOriginOffset);
        }
        public Vector3 GetOriginPosition(Vector3 offset)
        {
            return GetOriginPosition() + offset;
        }

        public Vector3 GetDefaultDirectionNormalized()
        {
            return Origin.TransformDirection(_defaultDirectionRelative).normalized;
        }

        public float GetDefaultDistance()
        {
            return _distance;
        }

        public abstract Vector3 GetCasterCenterOfHit(RaycastHit hit, float addedDistance);

        public Vector3 GetCentroidDistance(RaycastHit hit, Vector3 offsetUsed)
        {
            if (hit.distance == 0f && hit.point == Vector3.zero) return Vector3.zero; //An invalid hit that means that the cast was done from within the ground so CenterOfHit(hit) should be equal GetOriginPosition().
            else return GetCasterCenterOfHit(hit, _originDistanceOffset) - GetOriginPosition(offsetUsed);
        }
    
        /// <summary>
        /// Get the distance between the centroid of the hit and the centroid now.
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public Vector3 GetCentroidDistance(RaycastHit hit)
        {
            return GetCasterCenterOfHit(hit, _originDistanceOffset) - GetOriginPosition();
        }

        /// <summary>
        /// Did this cast was made from outside the casted collider?
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public virtual bool WasCastFromOutside(RaycastHit hit)
        {
            return hit.distance <= GetOutsideDistance();
        }

        /// <summary>
        /// Get the minimum distance that the hit.distance should return to prove that the cast was made from outside the intersection of the caster form and the hitted collider.
        /// </summary>
        /// <returns></returns>
        public abstract float GetOutsideDistance();

        public bool Cast()
        {
            RaycastHit hit;
            return CastAndValidate(GetOriginPosition(), Origin.TransformDirection(_defaultDirectionRelative), LayerMask, _distance, out hit);
        }

        public bool Cast(out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(), Origin.TransformDirection(_defaultDirectionRelative), LayerMask, _distance, out hit);
        }

        public bool Cast(Vector3 direction, out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(), direction, LayerMask, _distance, out hit);
        }

        public bool CastLength(Vector3 directionLength, out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(), directionLength, LayerMask, directionLength.magnitude, out hit);
        }

        public bool CastLengthOffset(Vector3 originOffset, Vector3 directionLength, out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(originOffset), directionLength, LayerMask, directionLength.magnitude, out hit);
        }

        public bool CastLength(Vector3 origin, Vector3 directionLength, out RaycastHit hit)
        {
            return CastAndValidate(origin, directionLength, LayerMask, directionLength.magnitude, out hit);
        }

        public bool Cast(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            return CastAndValidate(origin, direction, LayerMask, _distance, out hit);
        }

        public IEnumerable<RaycastHit> CastAll()
        {
            return CastAndValidateAll(GetOriginPosition(), Origin.TransformDirection(_defaultDirectionRelative), LayerMask, _distance);
        }

        public IEnumerable<RaycastHit> CastAll(Vector3 direction)
        {
            return CastAndValidateAll(GetOriginPosition(), direction, LayerMask, _distance);
        }

        public IEnumerable<RaycastHit> CastAllLength(Vector3 directionLength)
        {
            return CastAndValidateAll(GetOriginPosition(), directionLength, LayerMask, directionLength.magnitude);
        }

        public IEnumerable<RaycastHit> CastAllLengthOffset(Vector3 offset, Vector3 directionLength)
        {
            return CastAndValidateAll(GetOriginPosition(offset), directionLength, LayerMask, directionLength.magnitude);
        }


        protected void CorrectDistances(ref Vector3 origin, ref float distance, Vector3 directionNormalized)
        {
            origin -= directionNormalized * _safetyDistance;
            if(_originDistanceOffset != 0f)
            {
                Vector3 addedDistance = directionNormalized * _originDistanceOffset;
                if (!_normalizeOriginDistanceOffset) addedDistance *= distance;
                if (_restrictOriginDistanceOffsetToPlane != Vector3.zero) addedDistance = Vector3.ProjectOnPlane(addedDistance, transform.TransformDirection(_restrictOriginDistanceOffsetToPlane));

                origin += addedDistance;
            }

            distance += _safetyDistance;
        }
        private bool CheckCast(ref RaycastHit hit)
        {
            float hitdistanceold = hit.distance;
            //if (hitdistanceold == 0f && hit.point == Vector3.zero) return false;
            //hit.distance = Mathf.Max(0f, hitdistanceold - _safetyDistance);
            hit.distance = hitdistanceold - _safetyDistance;
            return HitMask.Contains(hit.collider.gameObject.layer);
        }

        private bool CheckCast(bool castOK, ref RaycastHit hit)
        {
            if (castOK)
            {
                return CheckCast(ref hit);
            }
            return false;
        }

        protected virtual bool CastAndValidate(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit)
        {
    #if UNITY_EDITOR
            Debug.DrawRay(origin, direction.normalized * distance, Color.red);
    #endif
            Vector3 directionNormalized = direction.normalized;
            CorrectDistances(ref origin, ref distance, directionNormalized);
            bool casted = CheckCast(MakeTheCast(origin, directionNormalized, mask, distance, out hit), ref hit);
        
            return casted;
        }

        protected virtual IEnumerable<RaycastHit> CastAndValidateAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance)
        {
    #if UNITY_EDITOR
            Debug.DrawRay(origin, direction.normalized * distance, Color.red);
    #endif
            Vector3 directionNormalized = direction.normalized;
            CorrectDistances(ref origin, ref distance, directionNormalized);
            int casted = MakeTheCastAll(origin, directionNormalized, mask, distance, HitsCache);

            for (int i = 0; i < casted; i++)
            {
                if (CheckCast(ref HitsCache[i]))
                {
                    yield return HitsCache[i];
                }
            }
        }

        protected abstract bool MakeTheCast(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit);

        protected abstract int MakeTheCastAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance, RaycastHit[] results);


        private void OnDrawGizmos()
        {
            Color gizmo = GizmosUtils.InformativeColor;

            if (Cast()) gizmo = GizmosUtils.SuccessColor;
            gizmo *= 0.25f;
            Gizmos.color = gizmo;
            DrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            Color gizmo = GizmosUtils.InformativeColor;

            if (Cast()) gizmo = GizmosUtils.SuccessColor;
        
            Gizmos.color = gizmo;
            DrawGizmos();
        }

        protected abstract void DrawGizmos();
    }
}
