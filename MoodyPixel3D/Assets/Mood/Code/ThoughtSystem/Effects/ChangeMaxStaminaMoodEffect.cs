using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Effect/Max Stamina", fileName = "Effect_MaxStamina_")]
public class ChangeMaxStaminaMoodEffect : ChangeFloatFromMoodPawnEffect
{
    protected override MoodParameter<float> GetParameterFromPawn(MoodPawn p)
    {
        return p._maxStamina;
    }
}
