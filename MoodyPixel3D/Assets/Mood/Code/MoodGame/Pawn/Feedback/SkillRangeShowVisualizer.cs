using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillRangeShowVisualizer : LHH.Unity.AddonParentBehaviour<MoodPawn>
{
    public RangeShow[] shows;

    private Coroutine _routine;

    private void OnEnable()
    {
        Addon.OnBeforeSkillUse += OnUseSkill;
        Addon.OnEndSkill += OnEndSkill;
    }


    private void OnDisable()
    {
        Addon.OnBeforeSkillUse -= OnUseSkill;
        Addon.OnEndSkill -= OnEndSkill;

        StopAllCoroutines();
        HideAll();
    }

    private void Start()
    {
        foreach (RangeShow show in shows)
        {
            show.Hide(null);
        }
    }

    protected void InterruptRoutine()
    {
        StopCoroutine(_routine);
    }


    private void OnUseSkill(MoodPawn pawn, MoodSkill skill, Vector3 skillDirection)
    {
        foreach (RangeShow show in shows)
        {
            if (show.CanShowSkill(skill))
            {
                _routine = StartCoroutine(show.ShowSkillLive(pawn, skill, skillDirection));
            }
        }
    }


    private void OnEndSkill(MoodPawn pawn, MoodSkill skill)
    {
        //StopAllCoroutines();
        //HideAll();
    }

    private void HideAll()
    {
        foreach (var show in shows) show.Hide(Addon);
    }
}
