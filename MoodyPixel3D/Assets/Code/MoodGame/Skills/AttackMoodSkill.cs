using System.Collections;
using LHH.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.MoodGame.Skills
{
    
    [CreateAssetMenu(fileName = "Skill_Attack_", menuName = "Mood/Skill/Attack", order = 0)]
    public class AttackMoodSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver, RangeTarget.IRangeShowPropertyGiver
    {
        [Header("Attack")]
        public int damage = 10;
        public float attackRadius = 0.5f;
        public float attackRange = 6f;
        public float attackY = 0.5f;
        public float attackCapsuleHeight = 1.5f;
        public LayerMask targetLayer;

        [Space] 
        public float preTime = 0.5f;
        public float postTime = 1f;
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
            foreach (Collider c in Physics.OverlapCapsule(downPoint, upPoint, attackRadius + 0.01f, targetLayer.value))
            {
                return c.transform.root;
            }

            return null;
        }

        public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
        {
            Target = pawn.FindTarget(direction, attackRange);
        }

        public override IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection)
        {
            pawn.StartSkillAnimation(this);
            yield return new WaitForSeconds(preTime);

            ExecuteEffect(pawn, skillDirection);
            DispatchExecuteEvent(pawn, skillDirection);
            
            pawn.FinishSkillAnimation(this);
            yield return new WaitForSeconds(postTime);
        }

        protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
        {
            Transform t = GetTarget(pawn.Position, skillDirection);
            if (t != null)
            {
                t.GetComponentInChildren<Health>()?.Damage(damage, pawn.DamageTeam);
            }

            return base.ExecuteEffect(pawn, skillDirection);
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
