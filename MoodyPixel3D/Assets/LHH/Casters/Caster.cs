﻿using System.Collections;
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
        private int _maxSimultaneousHits = 8;
        [SerializeField]
        private Transform _overridedOrigin;

        [Header("Offsets and distances")]
        [Tooltip("An amount of distance added moving the origin backwards so give leeway to hit a collider right in front.")]
        [SerializeField]
        protected float _safetyDistance = 0f;
        [Tooltip("Move the origin in an offset forward to simulate it actually beginning in an sphere around the origin.")]
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_originDistanceOffset")]
        protected float _originForwardOffset = 0f;
        [Tooltip("Restrict the origin distance offset to a plane dictated by a vector, relative to the caster's transform. If the plane is 0,0,0 nothing will happen.")]
        [SerializeField]
        protected Vector3 _restrictOriginDistanceOffsetToPlane;


        [Header("Parameterless calls values")]
        [SerializeField]
        private float _defaultDistance = 1f;
        [SerializeField]
        private Vector3 _defaultOriginOffset = Vector3.zero;
        [SerializeField]
        private Vector3 _defaultDirectionRelative = -Vector3.up;

        private RaycastHit[] _hitsAllArray;

        protected RaycastHit[] HitsCache
        {
            get
            {
                if (_hitsAllArray == null) _hitsAllArray = new RaycastHit[Mathf.NextPowerOfTwo(_maxSimultaneousHits)];
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
                if (_overridedOrigin == null) _overridedOrigin = transform;
                return _overridedOrigin;
            }
        }

        public float SafetyDistance
        {
            get
            {
                return _safetyDistance;
            }
        }

        public float CastDistanceOffset
        {
            get
            {
                return _originForwardOffset;
            }
        }

        public virtual Vector3 GetOriginPosition()
        {
            return Origin.TransformPoint(_defaultOriginOffset);
        }
        public Vector3 GetOriginPositionOffset(in Vector3 offset)
        {
            return GetOriginPosition() + offset;
        }

        protected Vector3 GetDefaultDirection()
        {
            return Origin.TransformDirection(_defaultDirectionRelative).normalized;
        }

        public Vector3 GetDefaultDirectionNormalized()
        {
            return GetDefaultDirection().normalized;
        }

        public float GetDefaultDistance()
        {
            return _defaultDistance;
        }

        public Vector3 GetDefaultDistanceLength()
        {
            return GetDefaultDirectionNormalized() * GetDefaultDistance();
        }


        /// <summary>
        /// Get the minimum relative distance vector that from this particular hit the caster would just be intersecting the point.
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        /// <returns></returns>
        public virtual Vector3 GetMinimumDistanceFromHit(in Vector3 hitPoint, in Vector3 hitNormal)
        {
            return GetSpecificMinimumDistanceFromHit(hitPoint, hitNormal) + hitNormal.normalized * _originForwardOffset;
        }

        /// <summary>
        /// Based on a hit, gets the relative distance where the center would be just grazing the surface.
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <param name="hitNormal"></param>
        /// <returns></returns>
        protected abstract Vector3 GetSpecificMinimumDistanceFromHit(in Vector3 hitPoint, in Vector3 hitNormal);

        /// <summary>
        /// Did this cast was made from outside the casted collider?
        /// </summary>
        /// <param name="hit"></param>
        /// <returns></returns>
        public virtual bool WasCastFromOutside(RaycastHit hit)
        {
            return hit.distance <= GetOutsideDistance(hit.normal);
        }

        /// <summary>
        /// Get the minimum distance that the hit.distance should return to prove that the cast was made from outside the intersection of the caster form and the hitted collider.
        /// </summary>
        /// <returns></returns>
        public abstract float GetOutsideDistance(Vector3 hitDirection);

        public bool Cast()
        {
            RaycastHit hit;
            return CastAndValidate(GetOriginPosition(), Origin.TransformDirection(_defaultDirectionRelative), LayerMask, _defaultDistance, out hit);
        }

        public bool Cast(out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(), Origin.TransformDirection(_defaultDirectionRelative), LayerMask, _defaultDistance, out hit);
        }

        public bool Cast(in Vector3 direction, out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(), direction, LayerMask, _defaultDistance, out hit);
        }

        public bool Cast(in Vector3 origin, in Vector3 direction, out RaycastHit hit)
        {
            return CastAndValidate(origin, direction, LayerMask, _defaultDistance, out hit);
        }

        public bool CastLength(in Vector3 direction, in float length, out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPosition(), direction, LayerMask, length, out hit);
        }

        public bool CastLengthOrigin(in Vector3 origin, in Vector3 direction, in float length, out RaycastHit hit)
        {
            return CastAndValidate(origin, direction, LayerMask, length, out hit);
        }

        public bool CastLengthOffset(in Vector3 originOffset, in Vector3 direction, in float length, out RaycastHit hit)
        {
            return CastAndValidate(GetOriginPositionOffset(originOffset), direction, LayerMask, length, out hit);
        }

        public IEnumerable<RaycastHit> CastAll()
        {
            return CastAndValidateAll(GetOriginPosition(), Origin.TransformDirection(_defaultDirectionRelative), LayerMask, _defaultDistance);
        }

        public IEnumerable<RaycastHit> CastAll(Vector3 direction)
        {
            return CastAndValidateAll(GetOriginPosition(), direction, LayerMask, _defaultDistance);
        }

        public IEnumerable<RaycastHit> CastAllLength(Vector3 directionLength)
        {
            return CastAndValidateAll(GetOriginPosition(), directionLength, LayerMask, directionLength.magnitude);
        }

        public IEnumerable<RaycastHit> CastAllLengthOffset(Vector3 offset, Vector3 directionLength)
        {
            return CastAndValidateAll(GetOriginPositionOffset(offset), directionLength, LayerMask, directionLength.magnitude);
        }


        protected void SanitizeCastParameters(ref Vector3 origin, ref float distance, Vector3 directionNormalized)
        {
            origin -= directionNormalized * _safetyDistance;
            if(_originForwardOffset != 0f)
            {
                Vector3 addedDistance = directionNormalized * _originForwardOffset;
                if (_restrictOriginDistanceOffsetToPlane != Vector3.zero) addedDistance = Vector3.ProjectOnPlane(addedDistance, transform.TransformDirection(_restrictOriginDistanceOffsetToPlane));

                origin += addedDistance;
            }

            distance += _safetyDistance;
        }
        private bool SanitizeHitResult(ref RaycastHit hit)
        {
            float hitdistanceold = hit.distance;
            hit.distance = hitdistanceold - _safetyDistance; //Yes distance can be less than 0 if the safety is too much (so it knows it should go back)
            return HitMask.Contains(hit.collider.gameObject.layer);
        }

        private bool CheckCast(bool castOK, ref RaycastHit hit)
        {
            if (castOK)
            {
                return SanitizeHitResult(ref hit);
            }
            return false;
        }

        protected virtual bool CastAndValidate(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit)
        {
            Vector3 directionNormalized = direction.normalized;
            SanitizeCastParameters(ref origin, ref distance, directionNormalized);

#if UNITY_EDITOR
            Debug.DrawRay(origin, directionNormalized * distance, Color.red);
            if(Application.isPlaying)
            {
                CasterDebugger.Instance.JustDoneCast(this, distance);
            }
#endif
            bool casted = CheckCast(MakeTheCast(origin, directionNormalized, mask, distance, out hit), ref hit);
        
            return casted;
        }

        protected virtual IEnumerable<RaycastHit> CastAndValidateAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance)
        {
    #if UNITY_EDITOR
            Debug.DrawRay(origin, direction.normalized * distance, Color.red);
            if (Application.isPlaying)
            {
                CasterDebugger.Instance.JustDoneCast(this, distance);
            }
#endif
            Vector3 directionNormalized = direction.normalized;
            SanitizeCastParameters(ref origin, ref distance, directionNormalized);
            int casted = MakeTheCastAll(origin, directionNormalized, mask, distance, HitsCache);

            for (int i = 0; i < casted; i++)
            {
                if (SanitizeHitResult(ref HitsCache[i]))
                {
                    yield return HitsCache[i];
                }
            }
        }

        protected abstract bool MakeTheCast(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit);

        protected abstract int MakeTheCastAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance, RaycastHit[] results);

        private void OnDrawGizmosSelected()
        {
            if(GetDefaultDistance() > 0f && GetDefaultDirection() != Vector3.zero)
                DrawGizmoForCast(GetOriginPosition(), GetDefaultDirection(), GetDefaultDistance(), out RaycastHit hit);
        }

        public void DrawGizmoForCast(in Vector3 origin, in Vector3 direction, in float distance, out RaycastHit hit)
        {
            Vector3 sanitizedOrigin = origin;
            float sanitizedDistance = distance;
            SanitizeCastParameters(ref sanitizedOrigin, ref sanitizedDistance, direction.normalized);

            Gizmos.color = GizmosUtils.InformativeColor * 0.5f;
            DrawFormatGizmo(origin);
            Gizmos.DrawWireSphere(origin, Mathf.Abs(_originForwardOffset));
            
            Gizmos.color = GizmosUtils.InformativeColor * 0.90f;
            DrawFormatGizmo(origin + direction * _originForwardOffset);

            Gizmos.color = GizmosUtils.InformativeColor * 0.75f;
            DrawFormatGizmo(sanitizedOrigin);
            Gizmos.color = GizmosUtils.InformativeColor * 0.90f;
            DrawFormatGizmo(sanitizedOrigin + direction * sanitizedDistance);


            if (CastLengthOrigin(origin, direction, distance, out hit))
            {
                Gizmos.color = GizmosUtils.SuccessColor * 0.5f;
                //SanitizeHitResult(ref hit);
                Vector3 destPosition = origin + direction.normalized * hit.distance;

                Gizmos.DrawLine(origin, destPosition);
                //GizmosUtils.DrawArrow(origin, destPosition, 0.5f);

                Gizmos.color = GizmosUtils.SuccessColor;
                DrawFormatGizmo(destPosition);
                //DrawFormatGizmo(GetCenterPositionOfHit(hit));
            }
            else
            {
                Gizmos.color = GizmosUtils.InformativeColor * 0.25f;
                DrawFormatGizmo(origin + direction * distance);
                Gizmos.DrawLine(origin, origin + direction * distance);
                GizmosUtils.DrawArrow(origin, origin + direction * distance, 0.5f);
            }
        }

        public abstract void DrawFormatGizmo(Vector3 position);
    }
}
