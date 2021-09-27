using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Caster
{
    public class RayCaster : Caster
    {
        public override void DrawFormatGizmo(Vector3 position)
        {
            LHH.Utils.GizmosUtils.DrawArrow(position, position + transform.forward * GetDefaultDistance(), 0.5f);
        }

        public override float GetOutsideDistance(Vector3 hitDirection)
        {
            return 0f;
        }

        protected override Vector3 GetSpecificMinimumDistanceFromHit(in Vector3 hitPoint, in Vector3 hitNormal)
        {
            return Vector3.zero;
        }

        protected override bool MakeTheCast(Vector3 origin, Vector3 direction, LayerMask mask, float distance, out RaycastHit hit)
        {
            return Physics.Raycast(origin, direction, out hit, distance, mask.value, QueryTriggerInteraction.UseGlobal);
        }

        protected override int MakeTheCastAll(Vector3 origin, Vector3 direction, LayerMask mask, float distance, RaycastHit[] results)
        {
            return Physics.RaycastNonAlloc(origin, direction, results, distance, mask.value, QueryTriggerInteraction.UseGlobal);
        }
    }
}