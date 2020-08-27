using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;

public static class MoodSharedBehaviourTypes
{
    [System.Serializable]
    public class SharedMoodPawn : SharedVariable<MoodPawn>
    {
        public static implicit operator SharedMoodPawn(MoodPawn value) { return new SharedMoodPawn { mValue = value }; }
    }
    [System.Serializable]
    public class SharedMoodSkill : SharedVariable<MoodSkill>
    {
        public static implicit operator SharedMoodSkill(MoodSkill value) { return new SharedMoodSkill { mValue = value }; }
    }
}
