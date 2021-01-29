using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Mood/Stances/Pawn Configuration", fileName ="PAWNSTANCES_")]
public class MoodPawnStanceConfiguration : ScriptableObject
{
    [Header("Flags")]
    public MoodEffectFlag onStaminaDownByAction;
    public MoodEffectFlag onStaminaDownByReaction;

    [Header("Default stances")]
    public ActivateableMoodStance[] flaggeableStances;
    public ConditionalMoodStance[] conditionalStances;
    public MoodReaction[] reactions;
}
