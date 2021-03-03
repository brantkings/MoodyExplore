using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSkillRange : LHH.Unity.AddonParentBehaviour<MoodPawn>
{
    public RangeShow[] shows;

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


    private void OnUseSkill(MoodPawn pawn, MoodSkill skill, Vector3 skillDirection)
    {
        foreach (RangeShow show in shows)
        {
            if (show.CanShowSkill(skill))
            {
                StartCoroutine(show.ShowSkillLive(pawn, skill, skillDirection));
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
