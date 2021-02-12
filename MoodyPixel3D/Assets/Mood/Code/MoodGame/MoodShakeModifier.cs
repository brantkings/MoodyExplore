using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(menuName = "Mood/Stances/Modifier/Shake", fileName = "StMod_Shake_")]
public class MoodShakeModifier : MoodPawnModifier
{
    public ShakeTweenData data;

    static Dictionary<MoodPawn, Tween> _tweens = new Dictionary<MoodPawn, Tween>();

    public override void ApplyModifier(MoodStance stance, MoodPawn pawn)
    {
        Transform shake = pawn.GetShakeTransform();
        if(shake != null)
        {
            _tweens.Add(pawn, data.ShakeTween(shake).SetLoops(-1));
        }
    }

    public override void RemoveModifier(MoodStance stance, MoodPawn pawn)
    {
        Tween t;
        if (_tweens.ContainsKey(pawn))
            t = _tweens[pawn];
        else t = null;

        if (t.IsNotNullAndMoving())
        {
            t.Kill();
            _tweens.Remove(pawn);
        }
    }
}
