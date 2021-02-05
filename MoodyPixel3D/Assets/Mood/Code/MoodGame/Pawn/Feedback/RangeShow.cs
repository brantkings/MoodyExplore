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

public interface IRangeShowDirected
{
    void SetDirection(Vector3 directionLength);
}

public abstract class RangeShow<T> : MonoBehaviour, IRangeShow
{
    public interface IRangeShowPropertyGiver
    {
        T GetRangeProperty();
    }

    public abstract void Show(T property);
    public abstract void Hide();

    public void ResolveSkill(MoodSkill skill)
    {
        //Debug.LogFormat("Does {0} implements {1}? {2}. Is it IRangeShowPropertyGiver? {3}", skill, typeof(T), skill.ImplementsRangeShow<T>(), skill is IRangeShowPropertyGiver);
        if (skill.ImplementsRangeShow<T>())
        {
            Show(skill.GetRangeShowProperty<T>().GetRangeProperty());
        }
        else
        {
            Hide();
        }
    }
}
