using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName ="Mood/Stances/Pawn Configuration", fileName ="PAWNSTANCES_")]
public class MoodPawnConfiguration : ScriptableObject
{
    public MoodPawnConfiguration parent;


    [Header("Flags")]
    public MoodEffectFlag onStaminaDownByAction;
    public MoodEffectFlag onStaminaDownByReaction;
    public MoodEffectFlag onDamage;

    [Header("Default stances")]
    public ActivateableMoodStance stanceOnSkill;
    public ActivateableMoodStance[] flaggeableStances;
    public ConditionalMoodStance[] conditionalStances;

    [Header("Default moves")]
    [SerializeField]
    private MoodSkill _equipMove;
    public MoodSkill EquipMove
    {
        get
        {
            if (_equipMove == null && parent != null) return parent.EquipMove;
            else return _equipMove;
        }
    }

    [SerializeField]
    private MoodSkill _unequipMove;
    public MoodSkill UnequipMove
    {
        get
        {
            if (_unequipMove == null && parent != null) return parent.UnequipMove;
            else return _unequipMove;
        }
    }

    [SerializeField]
    private MoodReaction[] reactions;
    [SerializeField]
    private MoodReaction[] reactionsLate;

    public ShakeTweenData shakeOnDamage;

    public IEnumerable<MoodReaction> GetReactions()
    {
        if(parent != null)
        {
            foreach (var r in reactions.Concat(parent.reactions).Distinct()) yield return r;
            foreach (var r in reactionsLate.Concat(parent.reactionsLate).Distinct()) yield return r;
        }
        else
        {
            foreach (var r in reactions.Distinct()) yield return r;
            foreach (var r in reactionsLate.Distinct()) yield return r;
        }
    }
}
