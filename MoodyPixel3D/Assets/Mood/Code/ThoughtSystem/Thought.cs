using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Thought System/Thought", fileName = "Thought_")]
public class Thought : ScriptableObject
{
    [SerializeField]
    private string thoughtPhrase;
    [TextArea]
    public string description;
    public Sprite thoughtIcon;
    public Color thoughtIconColor = Color.white;
    public Color thoughtPhraseColor = Color.white;
    [SerializeField]
    private ThoughtFocusable focusablePrefab;

    public bool consumedWhenExperienced = true;
    public int experienceNeeded;
    public MoodPawnEffect[] onUnfocused;
    public MoodPawnEffect[] onActivated;
    public MoodPawnEffect[] onExperienceComplete;
    public EquippableMoodItem[] onExperienceCompleteEquip;

    public virtual bool CheckExperienceCondition(MoodPawn pawn, ThoughtSystemController thought, int expCondition)
    {
        return expCondition >= experienceNeeded;
    }

    public virtual void AddThoughtInMind(MoodPawn pawn, ThoughtSystemController thought)
    {
        foreach (var effect in onUnfocused)
        {
            effect.AddEffect(pawn);
        }
    }

    public virtual void RemoveThoughtFromMind(MoodPawn pawn, ThoughtSystemController thought)
    {
        foreach (var effect in onUnfocused)
        {
            effect.RemoveEffect(pawn);
        }
    }

    public virtual IEnumerator FocusEffect(MoodPawn pawn, ThoughtSystemController thought)
    {
        foreach(var effect in onActivated)
        {
            effect.AddEffect(pawn);
        }
        foreach (var effect in onUnfocused)
        {
            effect.RemoveEffect(pawn);
        }
        yield break;
    }

    public virtual IEnumerator RemoveFocusEffect(MoodPawn pawn, ThoughtSystemController thought)
    {
        foreach(var effect in onActivated)
        {
            effect.RemoveEffect(pawn);
        }
        foreach (var effect in onUnfocused)
        {
            effect.AddEffect(pawn);
        }
        yield break;
    }

    public virtual IEnumerator ExperienceCompleteEffect(MoodPawn pawn, ThoughtSystemController thought)
    {
        RemoveFocusEffect(pawn, thought);

        foreach (var effect in onExperienceComplete)
        {
            effect.AddEffect(pawn);
        }

        foreach (var item in onExperienceCompleteEquip)
        {
            item.SetEquipped(pawn, true);
        }
        yield break;
    }

    public virtual ThoughtFocusable GetThoughtFocusablePrefab()
    {
        return focusablePrefab;
    }

    public string GetName()
    {
        return thoughtPhrase;
    }

    public string GetDescription()
    {
        return description;
    }
}
