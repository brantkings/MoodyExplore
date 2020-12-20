using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Skill/Skill Set", fileName = "SkillSet_", order = -1)]
public class MoodSkillSet : ScriptableObject
{
    public MoodSkill[] skills;
}
