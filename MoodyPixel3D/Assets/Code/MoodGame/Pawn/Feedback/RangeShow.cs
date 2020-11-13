using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IRangeShow
{
    void ResolveSkill(MoodSkill skill);
    void Hide();
}

public abstract class RangeShow<T> : MonoBehaviour, IRangeShow
{
    public interface IRangeShowPropertyGiver
    {
        T GetRangeProperty();
    }

    public abstract void Show(T property);
    public abstract void Hide();

    public IRangeShowPropertyGiver GetSkillProperty(MoodSkill skill)
    {
        return skill as IRangeShowPropertyGiver;
    }

    public void ResolveSkill(MoodSkill skill)
    {
        if (skill is IRangeShowPropertyGiver)
        {
            Show((skill as IRangeShowPropertyGiver).GetRangeProperty());
        }
        else
        {
            Hide();
        }
    }
}
