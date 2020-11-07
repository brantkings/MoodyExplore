using System.Collections;
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
        public LHH.Unity.MorphableProperty<KnockbackSolver> knockback;

        public SoundEffect onStartAttack;
        public SoundEffect onExecuteAttack;
        public SoundEffect onEndAttack;

        [Space] 
        public float preTime = 0.5f;
        public float postTime = 1f;
        private RangeTarget.Properties _targetProp;

        public MoodStance[] addedStancesWithAttack;

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

        public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
        {
            pawn.MarkUsingSkill(this);
            pawn.SetHorizontalDirection(skillDirection);
            pawn.StartThreatening(skillDirection);
            ConsumeStances(pawn);
            pawn.StartSkillAnimation(this);
            onStartAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            yield return new WaitForSeconds(preTime);

            float executingTime = ExecuteEffect(pawn, skillDirection);
            DispatchExecuteEvent(pawn, skillDirection);
            onExecuteAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            yield return new WaitForSecondsRealtime(executingTime);
            
            pawn.StopThreatening();
            pawn.FinishSkillAnimation(this);
            onEndAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            yield return new WaitForSeconds(postTime);
            pawn.UnmarkUsingSkill(this);
        }

        public override void Interrupt(MoodPawn pawn)
        {
            base.Interrupt(pawn);
            pawn.StopThreatening();
            pawn.FinishSkillAnimation(this);
        }

        protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
        {
            Transform t = pawn.FindTarget(skillDirection, attackRange);
            if (t != null)
            {
                t.GetComponentInChildren<Health>()?.Damage(GetDamage(pawn, t));
            }

            if(addedStancesWithAttack != null)
                foreach(MoodStance stance in addedStancesWithAttack)    
                    pawn.AddStance(stance);

            return base.ExecuteEffect(pawn, skillDirection);
        }

        private DamageInfo GetDamage(MoodPawn pawn, Transform target)
        {
            return new DamageInfo(damage, pawn.DamageTeam, pawn.gameObject).SetForce(knockback.Get().GetKnockback(pawn.transform, target), knockback.Get().GetDuration());
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
