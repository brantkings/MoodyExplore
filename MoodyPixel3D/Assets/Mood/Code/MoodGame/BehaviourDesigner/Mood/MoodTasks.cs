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

        public override TaskStatus OnUpdate()
        {
            Transform o = customOrigin.Value == null ? transform : customOrigin.Value;
            Transform d = PlayerTransform;
            if (o != null && d != null)
            {
                directionOut.Value = PlayerTransform.position - o.position;
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
    public class StopVelocity : Action
    {
        [SerializeField] private MoodSharedBehaviourTypes.SharedMoodPawn pawn;

        public override TaskStatus OnUpdate()
        {
            pawn.Value.SetVelocity(Vector3.zero);
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
            yield return pawn.ExecuteSkill(skill, direction);
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
            return skill.Value.CanExecute(pawn.Value, direction.Value) ? TaskStatus.Success : TaskStatus.Failure;
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

