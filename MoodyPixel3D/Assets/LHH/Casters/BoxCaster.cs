using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Caster
{
    public class BoxCaster : Caster
    {
        public enum OrientationType
        {
            Rotation,
            LocalRotation,
            AbsoluteValue,
            AlwaysIdentity
        }

        [Header("Box")]
        public Vector3 halfExtents;
        public OrientationType orientation = OrientationType.Rotation;
        public Vector3 eulerRotationMultiplier;

        private Quaternion ExtraMultiplier
        {
            get
            {
                return Quaternion.Euler(eulerRotationMultiplier);
            }
        }

        private Quaternion GetOrientation()
        {
            switch (orientation)
            {
                case OrientationType.Rotation:
                    return transform.rotation * ExtraMultiplier;
                case OrientationType.LocalRotation:
                    return transform.localRotation * ExtraMultiplier;
                case OrientationType.AbsoluteValue:
                    return ExtraMultiplier;
                default:
                    return Quaternion.identity;
            }
        }


        public override void DrawFormatGizmo(Vector3 position)
        {
            Utils.GizmosUtils.DrawBox(position, halfExtents, GetOrientation());
        }

        public override float GetOutsideDistance(Vector3 hitDirection)
        {
            Vector3? bestDistance = null;
            foreach(var planeNormal in GetAllPlanesNormal())
            {
                Vector3 distance = Vector3.Project(hitDirection, planeNormal);
                if(bestDistance.HasValue)
                {
                    if(distance.sqrMagnitude > bestDistance.Value.sqrMagnitude)
                    {
                        bestDistance = distance;
                    }
                }
                else
                {
                    bestDistance = distance;
                }
            }
            if (bestDistance.HasValue) return bestDistance.Value.magnitude;
            else return 0f;
        }

        protected override Vector3 GetSpecificMinimumDistanceFromHit(in Vector3 hitPoint, in Vector3 hitNormal)
        {
            return hitNormal * GetOutsideDistance(hitNormal);
        }

        protected override bool MakeTheCast(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit)
        {
            return Physics.BoxCast(origin, halfExtents, direction, out hit, GetOrientation(), distance, LayerMask.value, QueryTriggerInteraction.UseGlobal);
        }

        protected override int MakeTheCastAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance, RaycastHit[] results)
        {
            return Physics.BoxCastNonAlloc(origin, halfExtents, direction, results, GetOrientation(), distance, LayerMask.value, QueryTriggerInteraction.UseGlobal);
        }

        private IEnumerable<Vector3> GetAllPlanesNormal()
        {
            yield return GetOrientation() * Vector3.up;
            yield return GetOrientation() * Vector3.forward;
            yield return GetOrientation() * Vector3.right;
        }

    }
}