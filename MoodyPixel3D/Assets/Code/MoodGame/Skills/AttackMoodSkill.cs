using System.Collections;
using LHH.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.MoodGame.Skills
{
    
    [CreateAssetMenu(fileName = "Skill_Attack_", menuName = "Mood/Skill/Attack", order = 0)]
    public class AttackMoodSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver, RangeTarget.IRangeShowPropertyGiver
    {
        public int damage = 10;
        public float attackRadius = 0.5f;
        public float attackRange = 6f;
        public float attackY = 0.5f;
        public float attackCapsuleHeight = 1.5f;
        public LayerMask targetLayer;

        private RangeTarget.Properties _targetProp;

        private RangeTarget.Properties TargetProperties =>
            _targetProp ??= new RangeTarget.Properties()
            {
                radiusAround = attackRadius,
                target = null
            };

        private Transform Target
        {
            get => TargetProperties.target;
            set => TargetProperties.target = value;
        }

        private Transform GetTarget(Vector3 origin, Vector3 direction)
        {
            Vector3 downPoint = origin + direction + Vector3.up * attackY;
            Vector3 upPoint = origin + direction + Vector3.up * (attackY + attackCapsuleHeight);
            #if UNITY_EDITOR
            Debug.DrawLine(downPoint, upPoint, Color.magenta, 0.02f);
            DebugUtils.DrawNormalStar(downPoint, 0.25f, Quaternion.identity, Color.magenta, 0.02f);
            DebugUtils.DrawNormalStar(upPoint, 0.25f, Quaternion.identity, Color.magenta, 0.02f);
            #endif
            foreach (Collider c in Physics.OverlapCapsule(downPoint, upPoint, attackRadius + 0.01f, targetLayer.value))
            {
                return c.transform.root;
            }

            return null;
        }

        public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
        {
            Target = GetTarget(pawn.Position, direction);
        }

        public override IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection)
        {
            Transform t = GetTarget(pawn.Position, skillDirection);
            if (t != null)
            {
                Health enemy = t.GetComponentInChildren<Health>();
                enemy.Damage(damage, DamageTeam.Ally);
            }

            yield break;
        }

        RangeSphere.Properties RangeShow<RangeSphere.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
        {
            return new RangeSphere.Properties()
            {
                radius = attackRange
            };
        }

        RangeTarget.Properties RangeShow<RangeTarget.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
        {
            return TargetProperties;
        }
    }
}
