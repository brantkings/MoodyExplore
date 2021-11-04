using System;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DG.Tweening;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Mood
{

    public abstract class PlayerReferenceAction : Action
    {
        private static Transform _playerTransform;

        protected static Transform PlayerTransform
        {
            get
            {
                if (_playerTransform == null)
                {
                    _playerTransform = MoodPlayerController.Instance?.Pawn?.mover.transform;
                }

                return _playerTransform;
            }
        }
    }
    
    [TaskCategory("Mood")]
    public class GetDirectionToPlayer : PlayerReferenceAction
    {
        [SerializeField] private SharedTransform customOrigin;
        [SerializeField] private SharedVector3 directionOut;
        [SerializeField] private SharedBool normalized;

        public override TaskStatus OnUpdate()
        {
            Transform o = customOrigin.Value == null ? transform : customOrigin.Value;
            Transform d = PlayerTransform;
            if (o != null && d != null)
            {
                directionOut.Value = PlayerTransform.position - o.position;
                if (normalized.Value) directionOut.Value = directionOut.Value.normalized;
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }
    
    [TaskCategory("Mood/Pawn")]
    public class GetPawnDirectionToPlayer : PlayerReferenceAction
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 directionOut;

        public override TaskStatus OnUpdate()
        {
            Transform d = PlayerTransform;
            if (pawn.Value != null && d != null)
            {
                directionOut.Value = PlayerTransform.position - pawn.Value.Position;
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class IsTouchingWall : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;

        public override TaskStatus OnUpdate()
        {
            return (pawn.Value?.Walled).Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class IsGrounded : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;

        public override TaskStatus OnUpdate()
        {
            return (pawn.Value?.Grounded).Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
    
    [TaskCategory("Mood")]
    public class GetPlayerPosition : PlayerReferenceAction
    {
        [SerializeField] private SharedVector3 positionOut;

        public override TaskStatus OnUpdate()
        {
            Transform d = PlayerTransform;
            if (d != null)
            {
                positionOut.Value = PlayerTransform.position;
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }

    [TaskCategory("Mood/Pawn")]
    [TaskDescription("Rotate with pawn's rotation velocity.")]
    public class RotateTowardsDirection : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 direction;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.RotateTowards(direction.Value);
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class SetPawnDirection : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 direction;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.SetHorizontalDirection(direction.Value);
            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Mood/Pawn")]
    public class GetPawnDirection : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 outVector;

        public override TaskStatus OnUpdate()
        {
            outVector.Value = pawn.Value.Direction;
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class LookToDirection : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 direction;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.SetLookAt(direction.Value);
            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Mood/Pawn")]
    public class SetPawnVelocity : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 velocity;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.SetVelocity(velocity.Value);
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Mood/Pawn")]
    [TaskDescription("Move pawn in the direction for the amount needed. Pawn will rotate to accomodate.")]
    public class MoveDirection : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 movementPerSecond;
        [SerializeField] private SharedBool infinite;
        [SerializeField] private SharedFloat amount;

        private float amountWalked;

        public override void OnStart()
        {
            amountWalked = amount.Value;
        }

        public override TaskStatus OnUpdate()
        {
            pawn.Value.SetVelocity(movementPerSecond.Value);
            amountWalked -= Time.deltaTime * movementPerSecond.Value.magnitude;
            if (infinite.Value) return TaskStatus.Running;
            else
            {
                if (amountWalked <= 0f) return TaskStatus.Success;
                return TaskStatus.Running;
            }
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class StopVelocity : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedBool alsoCancelDash;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.SetVelocity(Vector3.zero);
            if (alsoCancelDash.Value) pawn.Value.CancelCurrentDash();
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class CancelDash : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.CancelCurrentDash();
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class Dash : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 relativePosition;
        [SerializeField] private SharedFloat velocity;
        //[SerializeField] private SharedAnimationCurve curve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
        [SerializeField] private Ease ease = Ease.Linear;
        [SerializeField] private VelocityKind useVelocityAs;

        private bool _dashed;

        public enum VelocityKind
        {
            Velocity,
            Duration
        }
        
        private float GetVelocity(float speedValue, Vector3 move, VelocityKind kind)
        {
            switch (useVelocityAs)
            {
                case VelocityKind.Velocity:
                    if (speedValue == 0f) 
                        return 0f;
                    else 
                        return move.magnitude / speedValue;
                default:
                    return speedValue;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (_dashed)
            {
                if (pawn.Value.IsDashing())
                {
                    return TaskStatus.Running;
                }
                else
                {
                    _dashed = false;
                    return TaskStatus.Success;
                }
            }
            if (pawn.Value != null)
            {
                _dashed = true;
                pawn.Value.Dash(relativePosition.Value, GetVelocity(velocity.Value, relativePosition.Value, useVelocityAs), ease);
                return TaskStatus.Running;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

    }

    [TaskCategory("Mood/Pawn")]
    public class GetWhereIAmTryingToGoNormal : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 outNormal;
        [SerializeField] private SharedBool notFindingAnythingIsNotFailure;
        public KinematicPlatformer.CasterClass caster = KinematicPlatformer.CasterClass.Side;

        public override TaskStatus OnUpdate()
        {
            if (pawn.Value.mover != null && pawn.Value.mover.WhatIsWhereIAmTryingToGo(caster, out RaycastHit hit))
            {
                outNormal.Value = hit.normal;
                return TaskStatus.Success;
            }
            else
            {
                outNormal.Value = Vector3.zero;
                if (notFindingAnythingIsNotFailure.Value) return TaskStatus.Success;
                else return TaskStatus.Failure;
            }
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class IsMovingHorizontally : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        public override TaskStatus OnUpdate()
        {
            Vector3 v = pawn.Value.Velocity;
            v.y = 0f;
            if (v.sqrMagnitude > 0f) return TaskStatus.Success;
            else return TaskStatus.Failure;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class IsMovingVertically : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        public override TaskStatus OnUpdate()
        {
            Vector3 v = pawn.Value.Velocity;
            if (v.y != 0f) return TaskStatus.Success;
            else return TaskStatus.Failure;
        }
    }

    [TaskCategory("Mood/Skill")]
    public class UseSkill : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodSkill skill;
        [SerializeField] private SharedVector3 direction;
        [SerializeField] private SharedBool keepRunningUntilCanUse;
        [SerializeField] private SharedBool waitUntilUsed = true;
        [SerializeField] private SharedBool directPawnToSkill = true;
        [SerializeField] private SharedBool sanitizeDirection = true;

        private bool _running;
        private bool _completed;

        public override void OnStart()
        {
            base.OnStart();
            _running = false;
            _completed = false;
        }

        public override TaskStatus OnUpdate()
        {
            
            //Debug.LogFormat("Executing {0} on {1}. Can execute? {2}, is it running? {3}",
                //this, this.transform, skill.Value.CanExecute(pawn.Value, direction.Value), _running);
            if (_running)
            {
                return TaskStatus.Running;
            }
            else
            {
                if (_completed)
                {
                    _completed = false;
                    return TaskStatus.Success;
                }
                
            }

            Vector3 skillDir = direction.Value;
            if(sanitizeDirection.Value)
            {
                skill.Value.SanitizeDirection(pawn.Value.Direction, ref skillDir);
            }
            if (skill.Value.CanExecute(pawn.Value, skillDir))
            {
                if(directPawnToSkill.Value) pawn.Value.SetHorizontalDirection(skillDir);
                StartCoroutine(UseSkillRoutine(skill.Value, pawn.Value, skillDir));
                if (waitUntilUsed.Value && _running) 
                    return TaskStatus.Running;
                else 
                    return TaskStatus.Success;
            } 
            else
            {
                if (keepRunningUntilCanUse.Value)
                {
                    return TaskStatus.Running;
                }
                else
                {
                    return TaskStatus.Failure;
                }
            }
        }

        public override void OnEnd()
        {
            if(_running && skill.Value != null) 
                skill.Value.Interrupt(pawn.Value);
            base.OnEnd();
        }

        private IEnumerator UseSkillRoutine(MoodSkill skill, MoodPawn pawn, Vector3 direction)
        {
            _running = true;
            _completed = false;
            pawn.OnInterruptSkill += OnInterruptSkill;
            yield return pawn.ExecuteSkill(skill, direction);
            _running = false;
            _completed = true;
        }

        private void OnInterruptSkill(MoodPawn pawn, MoodSkill skill)
        {
            pawn.OnInterruptSkill -= OnInterruptSkill;
            _running = false;
            _completed = true;
        }
    }
    
    [TaskCategory("Mood/Pawn")]
    public class IsThreatened : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;

        public override TaskStatus OnUpdate()
        {
            if (pawn.Value == null) return TaskStatus.Failure;
            return pawn.Value.Threatenable.IsThreatened() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
    
    [TaskCategory("Mood/Pawn")]
    public class IsNotThreatened : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;

        public override TaskStatus OnUpdate()
        {
            if (pawn.Value == null) return TaskStatus.Failure;
            return pawn.Value.Threatenable.IsThreatened() ? TaskStatus.Failure : TaskStatus.Success;
        }
    }

    

    [TaskCategory("Mood/Skill")]
    public class CanUseSkill : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodSkill skill;
        [SerializeField] private SharedVector3 direction;

        public override TaskStatus OnUpdate()
        {
            //Debug.LogFormat("{0} can execute {1} on {2}? {3}", pawn, skill, direction, skill.Value.CanExecute(pawn.Value, direction.Value));
            if (skill.Value == null) return TaskStatus.Failure;
            return skill.Value.CanExecute(pawn.Value, direction.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }

    [TaskCategory("Mood/Skill")]
    public class WillSkillHaveATarget : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodSkill skill;
        [SerializeField] private SharedVector3 direction;
        [SerializeField] private SharedBool considerNonApplicableAsOK = true;

        public override TaskStatus OnUpdate()
        {
            //Debug.LogFormat("{0} can execute {1} on {2}? {3}", pawn, skill, direction, skill.Value.CanExecute(pawn.Value, direction.Value));
            MoodSkill.WillHaveTargetResult result = skill.Value.WillHaveTarget(pawn.Value, direction.Value);
            switch (result)
            {
                case MoodSkill.WillHaveTargetResult.NonApplicable:
                    if (considerNonApplicableAsOK.Value) return TaskStatus.Success;
                    break;
                case MoodSkill.WillHaveTargetResult.NotHaveTarget:
                    return TaskStatus.Failure;
                case MoodSkill.WillHaveTargetResult.WillHaveTarget:
                    return TaskStatus.Success;
                default:
                    if (considerNonApplicableAsOK.Value) return TaskStatus.Success;
                    break;
            }
            return TaskStatus.Failure;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class HasStaminaForSkill : Conditional
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodSkill skill;

        public override TaskStatus OnUpdate()
        {
            StaminaCostMoodSkill stSkill = skill.Value as StaminaCostMoodSkill;
            if(stSkill != null && pawn.Value != null)
            {
                return stSkill.HasPawnEnoughStamina(pawn.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Failure;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class WaitUntilGetHit : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 outHitKnockback;
        [SerializeField] private SharedFloat outHitDuration;
        [SerializeField] private SharedInt outDamage;

        private bool damaged;

        public override void OnStart()
        {
            base.OnStart();
            damaged = false;
            pawn.Value.Health.OnDamage += OnDamage;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            pawn.Value.Health.OnDamage -= OnDamage;
        }

        public override TaskStatus OnUpdate()
        {
            if (!damaged) return TaskStatus.Running;
            else return TaskStatus.Success;
        }

        private void OnDamage(DamageInfo damage, Health damaged)
        {
            if (!outDamage.IsNone) outDamage.Value = damage.damage;
            if (!outHitKnockback.IsNone) outHitKnockback.Value = damage.distanceKnockback;
            if (!outHitDuration.IsNone) outHitDuration.Value = damage.durationKnockback;
            this.damaged = true;
        }
    }

    [TaskCategory("Mood/Pawn")]
    public class SearchTarget : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;
        [SerializeField] private SharedVector3 direction;
        [SerializeField] private SharedFloat range;
        [SerializeField] private SharedBool directionMagnitudeAsRange;
        [Space()] [SerializeField] private SharedGameObject targetGot;

        public override TaskStatus OnUpdate()
        {
            float usedRange = directionMagnitudeAsRange.Value ? direction.Value.magnitude : this.range.Value;
            targetGot.Value = pawn.Value.FindTarget(direction.Value, usedRange)?.gameObject;
            if (targetGot.Value != null)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }
}

