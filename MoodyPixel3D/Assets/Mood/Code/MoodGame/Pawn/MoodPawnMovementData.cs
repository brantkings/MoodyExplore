using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects;

[CreateAssetMenu(menuName = "Mood/Pawn/Movement Data", fileName = "MovementData_")]
public class MoodPawnMovementData : ScriptableObject, IDataSetter<MoodPawn.MovementData>, IDataSetter<MoodPawn.StaminaRecoveryData>
{
    [SerializeField] [UnityEngine.Serialization.FormerlySerializedAs("_data")] private MoodPawn.MovementData _movementData = MoodPawn.MovementData.Default;
    [SerializeField] private MoodPawn.StaminaRecoveryData _staminaRecoveryData = MoodPawn.StaminaRecoveryData.Default;

    public MoodPawn.MovementData MovementData { get => _movementData; set => _movementData = value; }
    public MoodPawn.StaminaRecoveryData StaminaRecoveryData { get => _staminaRecoveryData; set => _staminaRecoveryData = value; }

    MoodPawn.StaminaRecoveryData IDataSetter<MoodPawn.StaminaRecoveryData>.Data { get => _staminaRecoveryData; set => _staminaRecoveryData = value; }

    MoodPawn.MovementData IDataSetter<MoodPawn.MovementData>.Data { get => _movementData; set => _movementData = value; }

    MoodPawn.MovementData IData<MoodPawn.MovementData>.Data => _movementData;

    MoodPawn.StaminaRecoveryData IData<MoodPawn.StaminaRecoveryData>.Data => _staminaRecoveryData;
}
