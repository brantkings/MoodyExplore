using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects;

[CreateAssetMenu(menuName = "Mood/Pawn/Movement Data", fileName = "MovementData_")]
public class MoodPawnMovementData : ScriptableData<MoodPawn.MovementData>
{
    protected override MoodPawn.MovementData GetDefaultValue()
    {
        return MoodPawn.MovementData.Default;
    }
}
