using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects.Events;

namespace Code.MoodGame.Skills
{

    [CreateAssetMenu(fileName = "Skill_Attack_", menuName = "Mood/Skill/Attack", order = 0)]
    public class AttackMoodSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver, RangeTarget.IRangeShowPropertyGiver, RangeArea.IRangeShowPropertyGiver, RangeArea.IRangeShowLivePropertyGiver, RangeArrow.IRangeShowPropertyGiver
    {
        [Header("Attack")]
        public int damage = 10;
        public MoodUnitManager.TimeBeats stunTime = 4;
        public MoodSwing swingData;
        public Vector3 swingDataPositionOffset;
        public LayerMask targetLayer;
        public LHH.Unity.MorphableProperty<KnockbackSolver> knockback;
        public bool setDirection;
        public int priorityPreAttack = PRIORITY_NOT_CANCELLABLE;
        public int priorityAfterAttack = PRIORITY_CANCELLABLE;
        public int priorityAfterWhiff = PRIORITY_NOT_CANCELLABLE;
        public LHH.Unity.MorphableProperty<Thought> _painThought;
        public FlyingThought _flyingThought;

        [System.Serializable]
        protected struct DashStruct
        {
            public float distance;
            public bool bumpeable;
            public DirectionFixer angle;
            public DG.Tweening.Ease ease;
            [Space()]
            public float hopHeight;
            [Range(0f, 1f)]
            public float hopDurationRange;
            [Space()]
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
                    d.hopHeight = 0f;
                    d.hopDurationRange = 0.5f;
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
        protected DashStruct preAttackDash = DashStruct.DefaultValue;
        [SerializeField]
        protected DashStruct postAttackDash = DashStruct.DefaultValue;
        [SerializeField]
        protected DashStruct whiffAttackDash = DashStruct.DefaultValue;

        [Space()]
        public RangeArea.Properties.Positioning previewPositioning = RangeArea.Properties.Positioning.OriginalPositionPlusDirection;

        [Space]
        public MoodUnitManager.TimeBeats preDashDelay = 0;
        public MoodUnitManager.TimeBeats preTime = 4;
        public MoodUnitManager.TimeBeats animationTime = 1;
        public MoodUnitManager.TimeBeats postTime = 3;
        public bool showPreview;
        private RangeTarget.Properties _targetProp;

        [Space()]
        public string animationIntStepString = "Attack_Right";
        public SoundEffect onStartAttack;
        public SoundEffect onExecuteAttack;
        public SoundEffect onEndAttack;
        public ScriptableEventPositional[] onDamage;
        public ScriptableEventPositional[] onGuardedDamage;

        [Space]
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
            if (swingData != null) return swingData.GetBuildData(Quaternion.identity, swingDataPositionOffset).GetRange();
            else return 0f;
        }

        private Transform GetTarget(MoodPawn pawn, Vector3 origin, Vector3 direction)
        {
            return swingData.GetBuildData(pawn.ObjectTransform.rotation, GetSwingOffset(direction)).TryHitGetBest(pawn.ObjectTransform.TransformVector(swingDataPositionOffset) + origin, Quaternion.LookRotation(direction, Vector3.up), targetLayer, direction)?.collider.GetComponentInParent<MoodPawn>()?.transform;
        }

        public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
        {
            Target = pawn.FindTarget(GetSanitizerForFirstDash().Sanitize(direction, pawn.Direction), direction, swingData.GetBuildData(pawn, swingDataPositionOffset), targetLayer);
        }

        public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
        {
            if (!SanityCheck(pawn, skillDirection)) yield break;

            yield return new WaitForSeconds(preDashDelay);

            float preAttackDash = PrepareAttack(pawn, skillDirection, out MoodSwing.MoodSwingBuildData buildData);
            yield return new WaitForSeconds(preAttackDash);

            
            float executingTime = ExecuteAttack(pawn, skillDirection, buildData, out bool hit);

            yield return new WaitForSeconds(executingTime);

            PostHitDash(pawn, skillDirection, hit);

            yield return new WaitForSeconds(animationTime);

            PostAnimationEnd(pawn, skillDirection, hit);

            yield return new WaitForSeconds(postTime);

            FinishAttack(pawn, skillDirection, hit);

        }

        protected bool SanityCheck(MoodPawn pawn, in Vector3 skillDirection)
        {
            if (swingData == null)
            {
                Debug.LogErrorFormat("{0} has no swing data!", this);
                return false;
            }
            return true;
        }

        protected float PrepareAttack(MoodPawn pawn, in Vector3 skillDirection, out MoodSwing.MoodSwingBuildData buildData)
        {
            if (setDirection) pawn.SetHorizontalDirection(skillDirection);
            buildData = swingData.GetBuildData(pawn.ObjectTransform.rotation, GetSwingOffset(skillDirection));
            pawn.SetPlugoutPriority(priorityPreAttack);
            pawn.StartThreatening(skillDirection, buildData);
            ConsumeStances(pawn);
            float preAttackDuration = Mathf.Max(preTime, 0f);
            Dash(pawn, skillDirection, preAttackDash, preAttackDuration);
            pawn.SetAttackSkillAnimation(animationIntStepString, MoodPawn.AnimationPhase.PreAttack);
            onStartAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            return preAttackDuration;
        }

        protected float ExecuteAttack(MoodPawn pawn, in Vector3 skillDirection, in MoodSwing.MoodSwingBuildData buildData, out bool hit)
        {
            pawn.PrepareForSwing(buildData, skillDirection);
            pawn.SetAttackSkillAnimation(animationIntStepString, MoodPawn.AnimationPhase.PostAttack);
            pawn.ShowSwing(buildData, skillDirection);
            pawn.StopThreatening();

            onExecuteAttack.ExecuteIfNotNull(pawn.ObjectTransform);
            AddStances(pawn);
            hit = DealDamage(pawn, skillDirection);
            ExecutionResult success = hit ? ExecutionResult.Success : ExecutionResult.Failure;
            DispatchExecuteEvent(pawn, skillDirection, success);
            return ExecuteAttackEffect(pawn, skillDirection, success).Item1;
        }

        protected void PostHitDash(MoodPawn pawn, in Vector3 skillDirection, bool hit)
        {
            if (hit)
                Dash(pawn, skillDirection, postAttackDash, postTime + animationTime);
            else
                Dash(pawn, skillDirection, whiffAttackDash, postTime + animationTime);
        }

        protected void PostAnimationEnd(MoodPawn pawn, in Vector3 skillDirection, bool hit)
        {
            if (hit)
                pawn.SetPlugoutPriority(priorityAfterAttack);
            else
                pawn.SetPlugoutPriority(priorityAfterWhiff);

            onEndAttack.ExecuteIfNotNull(pawn.ObjectTransform);
        }

        protected void FinishAttack(MoodPawn pawn, in Vector3 skillDirection, bool hit)
        {
            pawn.SetAttackSkillAnimation(animationIntStepString, MoodPawn.AnimationPhase.None);
        }

        protected (float, ExecutionResult) ExecuteAttackEffect(MoodPawn pawn, Vector3 skillDirection, ExecutionResult success)
        {
            return MergeExecutionResult(base.ExecuteEffect(pawn, skillDirection), (0f, success));
        }

        protected void Dash(MoodPawn pawn, Vector3 skillDirection, DashStruct dashData, float duration)
        {
            if(dashData.HasDash())
            {
                pawn.Dash(dashData.GetDashDistance(pawn.Direction, skillDirection), duration, dashData.bumpeable, dashData.ease);
                if (dashData.hopHeight != 0f) pawn.Hop(dashData.hopHeight, new MoodPawn.TweenData(duration * dashData.hopDurationRange).SetEase(DG.Tweening.Ease.OutCirc), new MoodPawn.TweenData(duration * (1f - dashData.hopDurationRange)).SetEase(DG.Tweening.Ease.InCirc));
                dashData.Feedback(pawn.ObjectTransform, pawn, skillDirection);
            }
        }

        public override void Interrupt(MoodPawn pawn)
        {
            Debug.LogWarningFormat("Interrupted {0} on {1}", name, pawn.name);
            base.Interrupt(pawn);
            pawn.StopThreatening();
            pawn.SetAttackSkillAnimation(animationIntStepString, MoodPawn.AnimationPhase.None);
        }

        protected void AddStances(MoodPawn pawn)
        {
            if (addedStancesWithAttack != null)
                foreach (ActivateableMoodStance stance in addedStancesWithAttack)
                    pawn.AddStance(stance);
        }

        protected bool DealDamage(MoodPawn pawn, Vector3 skillDirection)
        {
            bool hit = false;
            foreach (MoodSwing.MoodSwingResult result in swingData.GetBuildData(pawn.ObjectTransform.rotation, GetSwingOffset(skillDirection)).TryHitMerged(pawn.Position, Quaternion.LookRotation(skillDirection, pawn.Up), targetLayer))
            {
                Debug.LogFormat("Result is {0}", result.collider);
                if (result.collider != null)
                {
                    if(!hit)
                    {
                        BattleLog.Log($"{pawn.GetName()} hits with '{GetName(pawn)}'!", BattleLog.LogType.Battle);
                    }

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
            if(!hit) BattleLog.Log($"{pawn.GetName()} whiffs!", BattleLog.LogType.Battle);
            return hit;
        }

        public override IEnumerable<MoodStance> GetStancesThatWillBeAdded()
        {
            foreach (MoodStance stance in addedStancesWithAttack) yield return stance;
        }

        protected DamageInfo GetDamage(MoodPawn pawn, Transform target, Vector3 attackDirection)
        {
            DamageInfo info = new DamageInfo(damage, pawn.DamageTeam, pawn.gameObject).SetStunTime(stunTime).SetForce(knockback.Get().GetKnockback(pawn.ObjectTransform, target, attackDirection, out float angle), angle, knockback.Get().GetDuration());
            if (_painThought != null) info.AddPainThought(new FlyingThoughtInstance() { flyingThought = _flyingThought, data = new FlyingThought.FlyingThoughtData() { destination = null, thought = _painThought, where = ThoughtSystemController.ThoughtPlacement.Down} });
            return info;
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
                skillPreviewSanitizer = GetSanitizerForFirstDash(),
                positioningWhenUsingSkill = previewPositioning,
            };
        }

        protected Vector3 GetSwingOffset(Vector3 skillDirection)
        {
            return swingDataPositionOffset;
        }

        public virtual bool ShouldShowNow(MoodPawn pawn)
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

        private RangeShow.SkillDirectionSanitizer GetClosedSanitizer()
        {
            return new RangeShow.SkillDirectionSanitizer(0f, 0f, DirectionFixer.LetAll);
        }

        private RangeShow.SkillDirectionSanitizer GetSanitizerForFirstDash()
        {
            return new RangeShow.SkillDirectionSanitizer(preAttackDash.distance, preAttackDash.distance, preAttackDash.angle);
        }

        public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
        {
            yield return preTime + preDashDelay;
            yield return animationTime + postTime;
        }

        public override WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection)
        {
            SanitizeDirection(pawn.Direction, ref skillDirection);
            MoodSwing.MoodSwingResult? result = swingData.GetBuildData(pawn, swingDataPositionOffset).TryHitGetFirst(pawn.Position + skillDirection.normalized * preAttackDash.distance, Quaternion.LookRotation(skillDirection), targetLayer);
            if (result.HasValue)
            {
                if (result.Value.IsValid()) return WillHaveTargetResult.WillHaveTarget;
            }
            return WillHaveTargetResult.NotHaveTarget;
        }
    }
}
