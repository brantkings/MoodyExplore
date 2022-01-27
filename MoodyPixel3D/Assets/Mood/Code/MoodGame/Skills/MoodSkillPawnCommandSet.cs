using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LHH.ScriptableObjects.Events;

public class MoodSkillPawnCommandSet : MoodSkill
{

    internal interface ICommandPart
    {
        IEnumerator DoCommandRoutine(MoodPawn pawn, MoodSkill skill, Vector3 input, float duration, RoutineState state);
        float GetDuration(MoodPawn pawn, in Vector3 input);
    }
    internal interface ITargetPart
    {
        bool WillHaveAtarget(MoodPawn pawn, Vector3 input);
    }


    [System.Serializable]
    public class Command
    {
        [SerializeField] internal int priorityInCommand = PRIORITY_NOT_CANCELLABLE;
        [SerializeField] internal float priortyChangeTimeProportional = 0.5f;
        [SerializeField] internal int priorityAdd = 0;
        [SerializeField] internal bool endCommandString = false;
        [SerializeField] internal DirectionFixer[] directionChange;


        internal enum WhatToDo
        {
            JustDelay,
            Movement,
            Attack,
            MovementAndAttack
        }
        [Space()]
        [SerializeField] internal WhatToDo whatToDo;





        [System.Serializable]
        internal class StateChangeAndFeedback : ICommandPart
        {
            [SerializeField] internal MoodUnitManager.TimeBeats delay;
            [SerializeField] internal bool setVelocityZero;
            [SerializeField] internal bool setDirectionToCommand;
            [SerializeField] internal ActivateableMoodStance[] stancesToAdd;
            [SerializeField] internal MoodEffectFlag[] flagsToAddBeginning;
            [SerializeField] internal MoodEffectFlag[] flagsToAddEnd;
            [SerializeField] internal ScriptableEvent[] events;
            [SerializeField] internal AnimatorID triggerAnim;
            [SerializeField] internal AnimatorID boolAnim;
            [SerializeField] internal bool boolAnimValue;
            [SerializeField] internal AnimatorID boolWhileInCommand;
            [SerializeField] internal AnimatorID animationStepInt;
            internal enum AnimationStepIntChange
            {
                Cancel,
                Increment,
                None,
            }
            [SerializeField] internal AnimationStepIntChange animationStepIntChange;
            [SerializeField] internal bool dispatchEvent;

            public IEnumerator DoCommandRoutine(MoodPawn pawn, MoodSkill skill, Vector3 input, float duration, RoutineState state)
            {
                if (setVelocityZero) pawn.SetVelocity(Vector3.zero);
                if (setDirectionToCommand) pawn.SetHorizontalDirection(input);
                if (stancesToAdd != null && stancesToAdd.Length > 0) for (int i = 0, len = stancesToAdd.Length; i < len; i++) pawn.AddStance(stancesToAdd[i]);
                pawn.AddFlags(flagsToAddBeginning);
                events.Invoke(pawn.ObjectTransform);

                if (triggerAnim.IsValid()) pawn.animator.SetTrigger(triggerAnim);
                if (boolWhileInCommand.IsValid()) pawn.animator.SetBool(boolWhileInCommand, true);
                if (boolAnim.IsValid()) pawn.animator.SetBool(boolAnim, boolAnimValue);
                if(animationStepInt.IsValid())
                {
                    switch (animationStepIntChange)
                    {
                        case AnimationStepIntChange.Cancel:
                            pawn.animator.SetInteger(animationStepInt, 0);
                            break;
                        case AnimationStepIntChange.Increment:
                            int step = pawn.animator.GetInteger(animationStepInt);
                            pawn.animator.SetInteger(animationStepInt, step + 1);
                            break;
                        default:
                            break;
                    }
                }

                yield return new WaitForSeconds(duration);

                pawn.AddFlags(flagsToAddEnd);
                if (boolWhileInCommand.IsValid()) pawn.animator.SetBool(boolWhileInCommand, false);


            }

            public float GetDuration(MoodPawn pawn, in Vector3 input)
            {
                return delay;
            }


        }
        [SerializeField] internal StateChangeAndFeedback feedback;

        internal class Movement : ICommandPart
        {
            [SerializeField] internal MoodUnitManager.DistanceBeats minDistance;
            [SerializeField] internal MoodUnitManager.DistanceBeats maxDistance;
            [SerializeField] internal bool movementIsBumpeable = true;
            [SerializeField] internal MoodUnitManager.SpeedBeats velocity = 1;
            [SerializeField] internal MoodUnitManager.TimeBeats durationAdd;
            [SerializeField] internal float showArrowWidth = 1f;
            [SerializeField] internal Ease ease;

            [System.Serializable]
            internal class Hop
            {
                public MoodUnitManager.DistanceBeats hopHeight;
                public float hopDurationInMultiplier;
                public float hopDurationOutMultiplier;
            }
            [SerializeField] internal Hop hopData;

            [Header("Feedback Movement")]
            public bool warningOnBumpWall;

            private Vector3 GetDistance(Vector3 input)
            {
                return input.Clamp(minDistance, maxDistance);
            }

            public IEnumerator DoCommandRoutine(MoodPawn pawn, MoodSkill skill, Vector3 input, float duration, RoutineState state)
            {
                Vector3 dist = GetDistance(input);
                pawn.Dash(GetDistance(input), false, duration, movementIsBumpeable, ease);
                if (hopData.hopHeight > 0) pawn.Hop(hopData.hopHeight, hopData.hopDurationInMultiplier, hopData.hopDurationOutMultiplier);
                state.movementDone += dist;
                yield break;
            }

            public float GetDuration(MoodPawn pawn, in Vector3 input)
            {
                return (GetDistance(input).magnitude / velocity.GetTotalLength()) + durationAdd.GetTotalLength();
            }
        }
        [SerializeField] internal Movement movement;

        [System.Serializable]
        internal class Attack : ICommandPart, ITargetPart
        {
            [SerializeField] internal LayerMask targetLayer;
            [SerializeField] internal int damage = 10;
            [SerializeField] internal MoodUnitManager.TimeBeats stunTime = 4;
            [SerializeField] internal MoodUnitManager.DistanceBeats knockbackDistance = 3;

            [Space]
            [SerializeField] internal MoodSwing swingData;
            [SerializeField] internal Vector3 swingDataPositionOffset;
            [SerializeField] internal LHH.Unity.MorphableProperty<KnockbackSolver> knockbackStyle;
            [SerializeField] internal LHH.Unity.MorphableProperty<Thought> _painThought;
            [SerializeField] internal FlyingThought _flyingThought;

            [Space]
            [SerializeField] internal int commandSkipOnSuccess = 0;

            public IEnumerator DoCommandRoutine(MoodPawn pawn, MoodSkill skill, Vector3 input, float duration, RoutineState state)
            {
                throw new System.NotImplementedException();
            }

            public float GetDuration(MoodPawn pawn, in Vector3 input)
            {
                throw new System.NotImplementedException();
            }

            public bool WillHaveAtarget(MoodPawn pawn, Vector3 input)
            {
                throw new System.NotImplementedException();
            }
        }
        [SerializeField] internal Attack attack;

        internal IEnumerable<ICommandPart> GetAllCommands()
        {
            yield return feedback;
            switch (whatToDo)
            {
                case WhatToDo.JustDelay:
                    
                    break;
                case WhatToDo.Movement:
                    yield return movement;
                    break;
                case WhatToDo.Attack:
                    yield return attack;
                    break;
                case WhatToDo.MovementAndAttack:
                    yield return movement;
                    yield return attack;
                    break;
                default:
                    break;
            }
        }

        internal IEnumerable<ITargetPart> GetTargetQuestions()
        {
            switch (whatToDo)
            {
                case WhatToDo.MovementAndAttack:
                    yield return attack;
                    break;
                case WhatToDo.Attack:
                    yield return attack;
                    break;
                default:
                    break;
            }
        }

    }

    [SerializeField] internal List<Command> commands;

    internal class RoutineState
    {
        internal Vector3 movementDone;
        internal bool dispatchedEvent;
    }

    public override WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection, MoodUnitManager.DistanceBeats distanceSafety)
    {
        bool applicable = false;
        for (int i = 0, len = commands.Count; i < len; i++)
        {
            Command command = commands[i];
            SanitizeDirection(pawn.Direction, ref skillDirection, command.directionChange);


            foreach (var stuff in command.GetTargetQuestions())
            {
                applicable = true;
                if (stuff.WillHaveAtarget(pawn, skillDirection))
                {
                    return WillHaveTargetResult.WillHaveTarget;
                }
            }

            if (command.endCommandString) break;
        }
        if (applicable) return WillHaveTargetResult.NotHaveTarget;
        else return WillHaveTargetResult.NonApplicable;
    }

    protected override (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, in CommandData skillDirection)
    {
        return (0f, ExecutionResult.Non_Applicable);
    }

    public override IEnumerator ExecuteRoutine(MoodPawn pawn, CommandData args)
    {
        RoutineState state = new RoutineState();
        for (int i = 0, len = commands.Count; i < len; i++)
        {
            Command command = commands[i];
            SanitizeDirection(pawn.Direction, ref args.direction, command.directionChange);;

            float duration = 0f;

            foreach(var stuff in command.GetAllCommands())
            {
                float stuffDuration = stuff.GetDuration(pawn, args.direction);
                pawn.StartCoroutine(stuff.DoCommandRoutine(pawn, this, args.direction, stuffDuration, state));

                duration = Mathf.Max(stuffDuration, duration);
            }

            yield return new WaitForSeconds(duration);
            if (command.endCommandString) break;
        }

        if (!state.dispatchedEvent) DispatchExecuteEvent(pawn, args, ExecutionResult.Non_Applicable);
    }

    public IEnumerator CommandRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        throw new System.NotImplementedException();
    }
}
