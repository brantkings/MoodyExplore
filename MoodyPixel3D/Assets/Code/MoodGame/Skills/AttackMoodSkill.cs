using System.Collections;
using UnityEngine;

namespace Code.MoodGame.Skills
{
    
    [CreateAssetMenu(fileName = "Skill_Attack_", menuName = "Mood/Skill/Attack", order = 0)]
    public class AttackMoodSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver, RangeTarget.IRangeShowPropertyGiver, RangeArea.IRangeShowPropertyGiver
    {
        [Header("Attack")]
        public int damage = 10;
        public MoodSwing swingData;
        public LayerMask targetLayer;
        public LHH.Unity.MorphableProperty<KnockbackSolver> knockback;
        public bool setDirection;

        public SoundEffect onStartAttack;
        public SoundEffect onExecuteAttack;
        public SoundEffect onEndAttack;
        public ScriptableEventPositional[] onDamage;

        [Space] 
        public float preTime = 0.5f;
        public float postTime = 1f;
        private RangeTarget.Properties _targetProp;

        public MoodStance[] addedStancesWithAttack;

        private RangeTarget.Properties TargetProperties =>
            _targetProp ??= new RangeTarget.Properties()
            {
                radiusAround = GetRange(),
                target = null
            };

        private Transform Target
        {
            get => TargetProperties.target;
            set => TargetProperties.target = value;
        }

        public float GetRange()
        {
            if (swingData != null) return swingData.GetRange();
            else return 0f;
        }

        private Transform GetTarget(Vector3 origin, Vector3 direction)
        {
            return swingData.TryHitGetBest(origin, Quaternion.LookRotation(direction, Vector3.up), targetLayer, direction)?.collider.GetComponentInParent<MoodPawn>()?.transform;
        }

        public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
        {
            Target = pawn.FindTarget(direction, swingData, targetLayer);
        }

        public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
        {
            if (swingData == null)
            {
                Debug.LogErrorFormat("{0} has no swing data!", this);
                yield break;
            }
            pawn.MarkUsingSkill(this);
            if(setDirection) pawn.SetHorizontalDirection(skillDirection);
            pawn.StartThreatening(skillDirection, swingData);
            ConsumeStances(pawn);
            pawn.StartSkillAnimation(this);
            onStartAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            yield return new WaitForSeconds(preTime);

            float executingTime = ExecuteEffect(pawn, skillDirection);
            DispatchExecuteEvent(pawn, skillDirection);
            onExecuteAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            pawn.StopThreatening();
            yield return new WaitForSecondsRealtime(executingTime);
            
            pawn.PrepareForSwing(swingData, skillDirection);
            pawn.FinishSkillAnimation(this);
            pawn.ShowSwing(swingData, skillDirection);
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

            Debug.LogFormat("Trying to find results! {0} {1} {2}", pawn, skillDirection, targetLayer.value);
            foreach (MoodSwing.MoodSwingResult result in swingData.TryHitMerged(pawn.Position, Quaternion.LookRotation(skillDirection, pawn.Up), targetLayer))
            {
                Debug.LogFormat("Result is {0}", result.collider);
                if(result.collider != null)
                {
                    Health enemyHealth = result.collider.GetComponentInParent<Health>();
                    Debug.LogFormat("{0} found collider {1}, health {2} in children {3}", this, result.collider, enemyHealth, result.collider.GetComponentInChildren<Health>());
                    enemyHealth?.Damage(GetDamage(pawn, result.collider.transform, result.hitDirection));
                    onDamage.Invoke(result.hitPosition, Quaternion.LookRotation(result.hitDirection, pawn.Up));
                }
            }

            if(addedStancesWithAttack != null)
                foreach(MoodStance stance in addedStancesWithAttack)    
                    pawn.AddStance(stance);

            return base.ExecuteEffect(pawn, skillDirection);
        }

        private DamageInfo GetDamage(MoodPawn pawn, Transform target, Vector3 attackDirection)
        {
            return new DamageInfo(damage, pawn.DamageTeam, pawn.gameObject).SetForce(knockback.Get().GetKnockback(pawn.ObjectTransform, target, attackDirection), knockback.Get().GetDuration());
        }

        RangeSphere.Properties RangeShow<RangeSphere.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
        {
            return new RangeSphere.Properties()
            {
                radius = GetRange()
            };
        }

        RangeTarget.Properties RangeShow<RangeTarget.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
        {
            return TargetProperties;
        }

        public RangeArea.Properties GetRangeProperty()
        {
            return new RangeArea.Properties()
            {
                swingData = this.swingData
            };
        }
    }
}
