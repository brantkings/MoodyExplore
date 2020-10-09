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
    public float preTime = 0.5f;
    public float postTime = 1f;
    private RangeTarget.Properties _targetProp;

    
    [SerializeField]
    private bool threatens;

    
    public SoundEffect onStartInstantiate;
    public SoundEffect onExecuteInstantiate;
    public SoundEffect onEndInstantiate;


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
        pawn.MarkUsingSkill(this);
        pawn.SetHorizontalDirection(skillDirection);
        pawn.StartSkillAnimation(this);
        onStartInstantiate.ExecuteIfNotNull(pawn.transform);
        if(threatens) pawn.StartThreatening(skillDirection);
        yield return new WaitForSeconds(preTime);

        ExecuteEffect(pawn, skillDirection);
        DispatchExecuteEvent(pawn, skillDirection);
        
        pawn.FinishSkillAnimation(this);
        if(threatens) pawn.StopThreatening();
        onExecuteInstantiate.ExecuteIfNotNull(pawn.transform);
        yield return new WaitForSeconds(postTime);
        pawn.UnmarkUsingSkill(this);
        onEndInstantiate.ExecuteIfNotNull(pawn.transform);
    }

    protected override float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection)
    {
        GameObject inst = Instantiate(prefab, pawn.GetInstantiatePlace(), pawn.GetInstantiateRotation());
        if (resetDamageTeamAsPawnTeam)
        {
            foreach (Damage damage in inst.GetComponentsInChildren<Damage>())
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

    /*RangeTarget.Properties RangeShow<RangeTarget.Properties>.IRangeShowPropertyGiver.GetRangeProperty()
    {
        return TargetProperties;
    }*/
}