using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Mood/Stances/Modifier/Animator", fileName = "StMod_Animator_")]
public class AnimatorMoodPawnModifier : MoodPawnModifier
{
    public enum WhatToDoComponent
    {
        Nothing,
        Deactivate
    }

    public WhatToDoComponent whatToDoWithAnimator;
    public float delay;

    static Dictionary<MoodPawn, Coroutine> _routinesOfThis = new Dictionary<MoodPawn, Coroutine>();

    public override void ApplyModifier(MoodStance stance, MoodPawn pawn)
    {
        if(delay > 0)
        {
            _routinesOfThis.Add(pawn, pawn.StartCoroutine(DelayDo(pawn, delay, true)));
        }
        else
        {
            Do(pawn.animator, true);
        }

    }

    private IEnumerator DelayDo(MoodPawn pawn, float delay, bool modifierOn)
    {
        yield return new WaitForSeconds(delay);
        Do(pawn.animator, modifierOn);
    }

    public override void RemoveModifier(MoodStance stance, MoodPawn pawn)
    {
        if (_routinesOfThis[pawn] != null) pawn.StopCoroutine(_routinesOfThis[pawn]);
        _routinesOfThis.Remove(pawn);
        Do(pawn.animator, false);
    }


    private void Do(Animator anim, bool modifierOn)
    {
        switch (whatToDoWithAnimator)
        {
            case WhatToDoComponent.Nothing:
                break;
            case WhatToDoComponent.Deactivate:
                anim.enabled = !modifierOn;
                break;
            default:
                break;
        }
    }

}
