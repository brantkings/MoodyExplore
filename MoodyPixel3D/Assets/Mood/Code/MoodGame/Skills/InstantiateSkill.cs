using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects.Events;

public abstract class InstantiateSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver
{
    [Header("Instantiate")]
    public bool setDirection = true;

    public float range = 50f;

    public bool resetDamageTeamAsPawnTeam;

    [System.Serializable]
    public struct ForceData
    {
        public Vector3 force;
        public ForceMode forceMode;
        public bool absoluteValue;

        public bool IsValid()
        {
            return force.sqrMagnitude > 0f;
        }

        public Vector3 GetForce(Transform origin)
        {
            return absoluteValue ? force : origin.rotation * force;
        }

        public static ForceData Default
        {
            get
            {
                return new ForceData()
                {
                    force = Vector3.zero,
                    forceMode = ForceMode.VelocityChange,
                    absoluteValue = false,
                };
            }
        }
    }


    [Space]
    public MoodUnitManager.TimeBeats preTime = 4;
    public MoodUnitManager.TimeBeats postTime = 8;
    private RangeTarget.Properties _targetProp;
    public int priorityPreInstantiate = PRIORITY_NOT_CANCELLABLE;
    public int priorityAfterInstantiate = PRIORITY_CANCELLABLE;


    [SerializeField]
    private MoodSwing threat;
    [SerializeField]
    private Vector3 threatOffset;
    [SerializeField]
    private ForceData forceOnThrow = ForceData.Default;


    public ScriptableEvent[] onStartInstantiate;
    public ScriptableEvent[] onExecuteInstantiate;
    public ScriptableEvent[] onEndInstantiate;


    private RangeTarget.Properties TargetProperties =>
        _targetProp ??= new RangeTarget.Properties()
        {
            radiusAround = 0f,
            target = null
        };

    private Transform Target
    {
        get => TargetProperties.target;
        set => TargetProperties.target = value;
    }

    /*private Transform GetTarget(Vector3 origin, Vector3 direction)
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
    }*/

    public override void SetShowDirection(MoodPawn pawn, Vector3 direction)
    {
        Target = pawn.FindTarget(direction, range);
    }

    public override IEnumerator ExecuteRoutine(MoodPawn pawn, CommandData command)
    {
        if (setDirection) pawn.SetHorizontalDirection(command.direction);
        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.PreAttack);
        pawn.SetPlugoutPriority(priorityPreInstantiate);
        onStartInstantiate.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(command.direction));
        if (threat != null) pawn.StartThreatening(command.direction, threat.GetBuildData(pawn, threatOffset));
        yield return new WaitForSeconds(preTime);

        ExecuteEffect(pawn, command);
        DispatchExecuteEvent(pawn, command, ExecutionResult.Success);

        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.PostAttack);
        pawn.SetPlugoutPriority(priorityAfterInstantiate);
        if (threat != null) pawn.StopThreatening();
        onExecuteInstantiate.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(command.direction));
        yield return new WaitForSeconds(postTime);
        onEndInstantiate.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(command.direction));
        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.None);
    }

    public override void Interrupt(MoodPawn pawn)
    {
        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.None);
        base.Interrupt(pawn);
    }

    protected override (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, in CommandData command)
    {
        GameObject inst = GetProjectile(pawn, command.direction, pawn.GetInstantiatePlace(), pawn.GetInstantiateRotation());
        if(inst != null)
        {
            inst.GetComponentInChildren<IMoodPawnSetter>()?.SetMoodPawnOwner(pawn);
            if (resetDamageTeamAsPawnTeam)
            {
                foreach (Damage damage in inst.GetComponentsInChildren<Damage>())
                {
                    damage.SetSourceDamageTeam(pawn.DamageTeam);
                }
            }

            if (forceOnThrow.IsValid())
            {
                Rigidbody instBody = inst.GetComponentInParent<Rigidbody>();
                if (instBody != null) instBody.AddForce(forceOnThrow.GetForce(pawn.ObjectTransform), forceOnThrow.forceMode);
            }

            return MergeExecutionResult(base.ExecuteEffect(pawn, command), (0f, ExecutionResult.Success));
        }
        else
        {
            return MergeExecutionResult(base.ExecuteEffect(pawn, command), (0f, ExecutionResult.Failure));
        }
    }

    RangeSphere.Properties RangeShow<RangeSphere.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
    {
        return new RangeSphere.Properties()
        {
            radius = range
        };
    }

    public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
    {
        yield return preTime;
        yield return postTime;
    }

    public override WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection, MoodUnitManager.DistanceBeats distanceSafety)
    {
        return WillHaveTargetResult.NonApplicable; //DIfficult to gauge if projectile will find opponent
    }

    /// <summary>
    /// Get the projectile for the skill. If it returns null, its ok, the pawn will not count items to be destroyed, etc.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="skillDirection"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <returns></returns>
    protected abstract GameObject GetProjectile(MoodPawn from, Vector3 skillDirection, Vector3 pos, Quaternion rot);
}
