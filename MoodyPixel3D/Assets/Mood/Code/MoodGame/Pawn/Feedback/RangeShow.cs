using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IRangeShow
{
    void ShowSkill(MoodPawn pawn, MoodSkill skill);
    void Hide(MoodPawn pawn);
}

public interface IRangeShowLive : IRangeShow
{
    IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill);
}

public interface IRangeShowDirected
{
    void SetDirection(Vector3 directionLength);
}

public abstract class RangeShow : MonoBehaviour, IRangeShowLive
{
    public abstract Type GetRangeType();
    public abstract void ShowSkill(MoodPawn pawn, MoodSkill skill);
    public abstract void Hide(MoodPawn pawn);
    public abstract IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill);

    public abstract bool CanShowSkill(MoodSkill skill);
}

public abstract class RangeShow<T> : RangeShow
{
    public interface IRangeShowPropertyGiver
    {
        T GetRangeProperty();
    }

    public interface IRangeShowLivePropertyGiver
    {
        bool ShouldShowNow(MoodPawn pawn);

    }

    public override Type GetRangeType()
    {
        return typeof(T);
    }

    public abstract void Show(MoodPawn pawn, T property);

    public override bool CanShowSkill(MoodSkill skill)
    {
        return skill.ImplementsRangeShow<T>();
    }

    public override void ShowSkill(MoodPawn pawn, MoodSkill skill)
    {
        //Debug.LogFormat("Does {0} implements {1}? {2}. Is it IRangeShowPropertyGiver? {3}", skill, typeof(T), skill.ImplementsRangeShow<T>(), skill is IRangeShowPropertyGiver);
        if (CanShowSkill(skill))
        {
            Show(pawn, skill.GetRangeShowProperty<T>().GetRangeProperty());
        }
        else
        {
            Hide(pawn);
        }
    }

    public override IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill)
    {
        IRangeShowLivePropertyGiver isLive = skill as IRangeShowLivePropertyGiver;
        if(isLive != null)
        {
            yield return CheckSkillRoutineLive(pawn, skill, isLive, skill as IRangeShowPropertyGiver);
        }
    }

    private IEnumerator CheckSkillRoutineLive(MoodPawn pawn, MoodSkill skill, RangeShow<T>.IRangeShowLivePropertyGiver isLive, RangeShow<T>.IRangeShowPropertyGiver propertyGetter)
    {
        bool repeat = true;
        while (repeat && pawn.GetCurrentSkill() == skill)
        {
            if (isLive.ShouldShowNow(pawn))
            {
                Show(pawn, propertyGetter.GetRangeProperty());
            }
            else
            {
                Hide(pawn);
            }
            yield return null;
            //Debug.LogFormat("[RANGESHOW] {0} is still showing {1} with {2}. Its current skill is {3}", pawn.name, skill.name, propertyGetter, pawn.GetCurrentSkill());
        }
        Hide(pawn);
    }
}
