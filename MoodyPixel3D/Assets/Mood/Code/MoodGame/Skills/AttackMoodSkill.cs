using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MoodGame.Skills
{
    
    [CreateAssetMenu(fileName = "Skill_Attack_", menuName = "Mood/Skill/Attack", order = 0)]
    public class AttackMoodSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver, RangeTarget.IRangeShowPropertyGiver, RangeArea.IRangeShowPropertyGiver, RangeArea.IRangeShowLivePropertyGiver, RangeArrow.IRangeShowPropertyGiver
    {
        [Header("Attack")]
        public int damage = 10;
        public TimeBeatManager.BeatQuantity stunTime = 4;
        public MoodSwing swingData;
        public Vector3 swingDataPositionOffset;
        public LayerMask targetLayer;
        public LHH.Unity.MorphableProperty<KnockbackSolver> knockback;
        public bool setDirection;
        public int priorityPreAttack = PRIORITY_NOT_CANCELLABLE;
        public int priorityAfterAttack = PRIORITY_CANCELLABLE;
        public int priorityAfterWhiff = PRIORITY_NOT_CANCELLABLE;

        [System.Serializable]
        private struct DashStruct
        {
            public float distance;
            public DirectionFixer angle;
            public DG.Tweening.Ease ease;
            public ScriptableEvent[] dashFeedback;
            public enum Direction
            {
                PawnDirection,
                DashDirection
            }
            public Direction dashFeedbackDirection;


            public void Feedback(Transform t, MoodPawn pawn, Vector3 direction)
            {
                switch (dashFeedbackDirection)
                {
                    case Direction.PawnDirection:
                        dashFeedback.Invoke(t, pawn.Position, Quaternion.LookRotation(pawn.Direction));
                        break;
                    default:
                        dashFeedback.Invoke(t, pawn.Position, Quaternion.LookRotation(direction));
                        break;
                }
            }

            public static DashStruct DefaultValue
            {
                get
                {
                    DashStruct d = new DashStruct();
                    d.distance = 0f;
                    d.angle = DirectionFixer.LetAll;
                    d.ease = DG.Tweening.Ease.OutCirc;
                    return d;
                }
            }

            public Vector3 GetDashDistance(Vector3 pawnDirection, Vector3 skillDirection)
            {
                return angle.Sanitize(skillDirection, pawnDirection).normalized * distance;
            }

            public bool HasDash()
            {
                return distance != 0f;
            }
        }

        [Space()]
        [SerializeField]
        private DashStruct preAttackDash = DashStruct.DefaultValue;
        [SerializeField]
        private DashStruct postAttackDash = DashStruct.DefaultValue;
        [SerializeField]
        private DashStruct whiffAttackDash = DashStruct.DefaultValue;

        [Space()]
        public SoundEffect onStartAttack;
        public SoundEffect onExecuteAttack;
        public SoundEffect onEndAttack;
        public ScriptableEventPositional[] onDamage;
        public ScriptableEventPositional[] onGuardedDamage;

        [Space] 
        public TimeBeatManager.BeatQuantity preTime = 4;
        public TimeBeatManager.BeatQuantity animationTime = 1;
        public TimeBeatManager.BeatQuantity postTime = 3;
        public bool showPreview;
        private RangeTarget.Properties _targetProp;

        public ActivateableMoodStance[] addedStancesWithAttack;

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
            if (swingData != null) return swingData.GetRange(swingDataPositionOffset);
            else return 0f;
        }

        private Transform GetTarget(MoodPawn pawn, Vector3 origin, Vector3 direction)
        {
            return swingData.TryHitGetBest(pawn.ObjectTransform.TransformVector(swingDataPositionOffset) + origin, Quaternion.LookRotation(direction, Vector3.up), targetLayer, direction)?.collider.GetComponentInParent<MoodPawn>()?.transform;
        }

        public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
        {
            Target = pawn.FindTarget(GetSanitizerForFirstDash().Sanitize(direction, pawn.Direction), direction, swingData, targetLayer);
        }

        public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
        {
            if (swingData == null)
            {
                Debug.LogErrorFormat("{0} has no swing data!", this);
                yield break;
            }
            MoodSwing.MoodSwingBuildData buildData = swingData.GetBuildData(pawn.ObjectTransform.rotation, swingDataPositionOffset);
            pawn.SetPlugoutPriority(priorityPreAttack);
            if(setDirection) pawn.SetHorizontalDirection(skillDirection);
            pawn.StartThreatening(skillDirection, swingData);
            ConsumeStances(pawn);
            yield return null;
            pawn.SetAttackSkillAnimation("Attack_Right", MoodPawn.AnimationPhase.PreAttack);
            onStartAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            float preAttackDuration = Mathf.Max(preTime - Time.deltaTime, 0f);
            Dash(pawn, skillDirection, preAttackDash, preAttackDuration);
            yield return new WaitForSeconds(preAttackDuration);

            pawn.PrepareForSwing(buildData, skillDirection);
            pawn.SetAttackSkillAnimation("Attack_Right", MoodPawn.AnimationPhase.PostAttack);
            pawn.ShowSwing(buildData, skillDirection);

            DispatchExecuteEvent(pawn, skillDirection);
            onExecuteAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            pawn.StopThreatening();
            AddStances(pawn);
            bool hit = DealDamage(pawn, skillDirection);
            float executingTime = ExecuteEffect(pawn, skillDirection);
            yield return new WaitForSecondsRealtime(executingTime);
            if(hit)
                Dash(pawn, skillDirection, postAttackDash, postTime + animationTime);
            else
                Dash(pawn, skillDirection, whiffAttackDash, postTime + animationTime);
            yield return new WaitForSeconds(animationTime);
            if (hit)
                pawn.SetPlugoutPriority(priorityAfterAttack);
            else
                pawn.SetPlugoutPriority(priorityAfterWhiff);

            onEndAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            yield return new WaitForSeconds(postTime);

            pawn.SetAttackSkillAnimation("Attack_Right", MoodPawn.AnimationPhase.None);
        }

        private void Dash(MoodPawn pawn, Vector3 skillDirection, DashStruct dashData, float duration)
        {
            if(dashData.HasDash())
            {
                pawn.Dash(dashData.GetDashDistance(pawn.Direction, skillDirection), duration, dashData.ease);
                dashData.Feedback(pawn.ObjectTransform, pawn, skillDirection);
            }
        }

        public override void Interrupt(MoodPawn pawn)
        {
            Debug.LogWarningFormat("Interrupted {0} on {1}", name, pawn.name);
            base.Interrupt(pawn);
            pawn.StopThreatening();
            pawn.SetAttackSkillAnimation("Attack_Right", MoodPawn.AnimationPhase.None);
        }

        private void AddStances(MoodPawn pawn)
        {
            if (addedStancesWithAttack != null)
                foreach (ActivateableMoodStance stance in addedStancesWithAttack)
                    pawn.AddStance(stance);
        }

        private bool DealDamage(MoodPawn pawn, Vector3 skillDirection)
        {
            bool hit = false;
            foreach (MoodSwing.MoodSwingResult result in swingData.TryHitMerged(pawn.Position, Quaternion.LookRotation(skillDirection, pawn.Up), targetLayer))
            {
                //Debug.LogFormat("Result is {0}", result.collider);
                if (result.collider != null)
                {
                    hit = true;

                    Debug.LogWarningFormat("Attack {0} found collider {1} from '{2}'", name, result.collider.name, result.collider.GetComponentInParent<MoodPawn>()?.name);
                    Health enemyHealth = result.collider.GetComponentInParent<Health>();
                    //Debug.LogFormat("{0} found collider {1}, health {2} in children {3}", this, result.collider, enemyHealth, result.collider.GetComponentInChildren<Health>());
                    Health.DamageResult? damageResult = enemyHealth?.Damage(GetDamage(pawn, result.collider.transform, result.hitDirection));

                    if (damageResult.HasValue)
                    {
                        switch (damageResult.Value)
                        {
                            case Health.DamageResult.Nothing:
                                break;
                            case Health.DamageResult.DamagingHit:
                                onDamage.Invoke(result.hitPosition, Quaternion.LookRotation(result.hitDirection, pawn.Up));
                                break;
                            case Health.DamageResult.NotDamagingHit:
                                onGuardedDamage.Invoke(result.hitPosition, Quaternion.LookRotation(result.hitDirection, pawn.Up));
                                break;
                            case Health.DamageResult.KillingHit:
                                onDamage.Invoke(result.hitPosition, Quaternion.LookRotation(result.hitDirection, pawn.Up));
                                break;
                            case Health.DamageResult.HealHit:
                                onGuardedDamage.Invoke(result.hitPosition, Quaternion.LookRotation(result.hitDirection, pawn.Up));
                                break;
                            default:
                                break;
                        }
                    }

                }
            }
            return hit;
        }

        public override IEnumerable<MoodStance> GetStancesThatWillBeAdded()
        {
            foreach (MoodStance stance in addedStancesWithAttack) yield return stance;
        }

        private DamageInfo GetDamage(MoodPawn pawn, Transform target, Vector3 attackDirection)
        {
            return new DamageInfo(damage, pawn.DamageTeam, pawn.gameObject).SetStunTime(stunTime).SetForce(knockback.Get().GetKnockback(pawn.ObjectTransform, target, attackDirection, out float angle), angle, knockback.Get().GetDuration());
        }

        RangeSphere.Properties RangeShow<RangeSphere.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
        {
            return new RangeSphere.Properties()
            {
                radius = GetRange() + preAttackDash.distance
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
                swingData = this.swingData,
                offset = swingDataPositionOffset,
                skillDirectionBeginning = GetSanitizerForFirstDash(),
            };
        }

        public bool ShouldShowNow(MoodPawn pawn)
        {
            return pawn.GetTimeElapsedSinceBeganCurrentSkill() < preTime;
            //return showPreview;
        }

        RangeArrow.Properties RangeShow<RangeArrow.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
        {
            return new RangeArrow.Properties()
            {
                directionFixer = GetSanitizerForFirstDash(),
                width = 0.5f
            };
        }

        private RangeShow.SkillDirectionSanitizer GetSanitizerForFirstDash()
        {
            return new RangeShow.SkillDirectionSanitizer(preAttackDash.distance, preAttackDash.distance, preAttackDash.angle);
        }

        public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
        {
            yield return preTime;
            yield return animationTime + postTime;
        }
    }
}
