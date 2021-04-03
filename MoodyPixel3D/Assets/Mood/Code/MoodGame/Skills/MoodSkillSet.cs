using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Skill/Skill Set", fileName = "SkillSet_", order = -1)]
public class MoodSkillSet : ScriptableObject, IEnumerable<MoodSkill>
{
    public MoodSkill[] skills;

    public IEnumerator<MoodSkill> GetEnumerator()
    {
        for (int i = 0, l = skills.Length; i < l; i++) yield return skills[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return skills.GetEnumerator();
    }
}
