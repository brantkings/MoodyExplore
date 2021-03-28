using System.Collections;
using System.Collections.Generic;
using LHH.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Instantiate_", menuName = "Mood/Skill/Instantiate", order = 0)]
public class InstantiateSkill : StaminaCostMoodSkill, RangeSphere.IRangeShowPropertyGiver
{
    [Header("Instantiate")] 
    public GameObject prefab;

    public float range = 50f;

    public bool resetDamageTeamAsPawnTeam;
    

    [Space] 
    public TimeBeatManager.BeatQuantity preTime = 4;
    public TimeBeatManager.BeatQuantity postTime = 8;
    private RangeTarget.Properties _targetProp;
    public int priorityPreInstantiate = PRIORITY_NOT_CANCELLABLE;
    public int priorityAfterInstantiate = PRIORITY_CANCELLABLE;

    
    [SerializeField]
    private MoodSwing threat;

    
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

    public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        pawn.SetHorizontalDirection(skillDirection);
        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.PreAttack);
        pawn.SetPlugoutPriority(priorityPreInstantiate);
        onStartInstantiate.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(skillDirection));
        if(threat != null) pawn.StartThreatening(skillDirection, threat);
        yield return new WaitForSeconds(preTime);

        ExecuteEffect(pawn, skillDirection);
        DispatchExecuteEvent(pawn, skillDirection);

        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.PostAttack);
        pawn.SetPlugoutPriority(priorityAfterInstantiate);
        if(threat != null) pawn.StopThreatening();
        onExecuteInstantiate.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(skillDirection));
        yield return new WaitForSeconds(postTime);
        onEndInstantiate.Invoke(pawn.ObjectTransform, pawn.Position, Quaternion.LookRotation(skillDirection));
        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.None);
    }

    public override void Interrupt(MoodPawn pawn)
    {
        pawn.SetAttackSkillAnimation("Attack_Left", MoodPawn.AnimationPhase.None);
        base.Interrupt(pawn);
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        GameObject inst = Instantiate(prefab, pawn.GetInstantiatePlace(), pawn.GetInstantiateRotation());
        inst?.GetComponentInChildren<IMoodPawnSetter>()?.SetMoodPawnOwner(pawn);
        if (resetDamageTeamAsPawnTeam)
        {
            foreach (Damage damage in inst?.GetComponentsInChildren<Damage>())
            {
                damage.SetSourceDamageTeam(pawn.DamageTeam);
            }
        }
        return base.ExecuteEffect(pawn, skillDirection);
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

    /*RangeTarget.Properties RangeShow<RangeTarget.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
    {
        return TargetProperties;
    }*/
}