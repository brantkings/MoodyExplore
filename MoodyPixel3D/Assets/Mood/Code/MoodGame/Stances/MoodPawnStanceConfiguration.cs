using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Mood/Stances/Pawn Configuration", fileName ="PAWNSTANCES_")]
public class MoodPawnStanceConfiguration : ScriptableObject
{
    [Header("Flags")]
    public MoodEffectFlag onStaminaDownByAction;
    public MoodEffectFlag onStaminaDownByReaction;
    public MoodEffectFlag onDamage;

    [Header("Default stances")]
    public ActivateableMoodStance stanceOnSkill;
    public ActivateableMoodStance[] flaggeableStances;
    public ConditionalMoodStance[] conditionalStances;

    [SerializeField]
    private MoodReaction[] reactions;
    [SerializeField]
    private MoodReaction[] reactionsLate;

    public IEnumerable<MoodReaction> GetReactions()
    {
        foreach (var r in reactions) yield return r;
        foreach (var r in reactionsLate) yield return r;
    }
}
