using System.Collections;
using System.Collections.Generic;
using LHH.Utils;
using UnityEngine;
using LHH.ScriptableObjects.Events;

[CreateAssetMenu(fileName = "Skill_Instantiate_", menuName = "Mood/Skill/Instantiate", order = 0)]
public class InstantiatePrefabSkill : InstantiateSkill
{
    [Header("Instantiate")] 
    public GameObject prefab;

    protected override GameObject GetProjectile(MoodPawn from, Vector3 skillDirection, Vector3 pos, Quaternion rot)
    {
        return Instantiate(prefab, pos, rot);
    }
}