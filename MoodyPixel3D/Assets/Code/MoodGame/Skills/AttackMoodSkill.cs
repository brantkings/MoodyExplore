using UnityEngine;

namespace Code.MoodGame.Skills
{
    
    [CreateAssetMenu(fileName = "Skill_Attack_", menuName = "Mood/Skill/Attack", order = 0)]
    public class AttackMoodSkill : StaminaCostMoodSkill, IRangeSphereSkill
    {
        public override void Execute(MoodPawn pawn, Vector3 skillDirection)
        {
            
        }

        public RangeSphere.Properties GetRangeSphereProperties()
        {
            return new RangeSphere.Properties()
            {
                radius = 6f
            };
        }
    }
}
