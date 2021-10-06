using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.MoodGame.Skills
{
    [CreateAssetMenu(fileName = "Skill_AttackOnTouch_", menuName = "Mood/Skill/AttackOnTouch", order = 0)]
    public class AttackMoodSkill_OnTouch : AttackMoodSkill
    {
        [Header("Ou Touch modifier")]

        public TimeBeatManager.BeatQuantity timeUntilStartsTesting;
        public override IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
        {
            if (!SanityCheck(pawn, skillDirection)) yield break;

            yield return new WaitForSeconds(preDashDelay);

            float preAttackDashDuration = PrepareAttack(pawn, skillDirection, out MoodSwing.MoodSwingBuildData buildData);

            float count = 0f;

            bool shouldBreak = false;
            MoodPawn.PawnEvent onDash = () =>
            {
                shouldBreak = true;
            };
            while (preAttackDashDuration > 0f && !shouldBreak)
            {
                if(count > timeUntilStartsTesting.GetTime())
                {
                    pawn.OnNextEndMove += onDash;
                    bool? valid = buildData.TryHitGetFirst(pawn.Position, pawn.GetRotation(), targetLayer)?.IsValid();
                    if (valid.HasValue && valid.Value)
                    {
                        break;
                    }
                }

                yield return new WaitForFixedUpdate();
                preAttackDashDuration -= Time.fixedDeltaTime;
                count += Time.fixedDeltaTime;
            }

            pawn.OnNextEndMove -= onDash;
            

            float executingTime = ExecuteAttack(pawn, skillDirection, buildData, out bool hit);

            yield return new WaitForSecondsRealtime(executingTime);

            PostHitDash(pawn, skillDirection, hit);

            yield return new WaitForSeconds(animationTime);

            PostAnimationEnd(pawn, skillDirection, hit);

            yield return new WaitForSeconds(postTime);

            FinishAttack(pawn, skillDirection, hit);
        }

        private void Pawn_OnEndMove()
        {
            throw new System.NotImplementedException();
        }

        public override bool ShouldShowNow(MoodPawn pawn)
        {
            float timeSinceBegin = pawn.GetTimeElapsedSinceBeganCurrentSkill();
            //bool isTesting = timeSinceBegin >= (timeUntilStartsTesting + preDashDelay) && timeSinceBegin <= (preTime + preDashDelay);
            bool isTesting = timeSinceBegin >= preDashDelay && timeSinceBegin <= (preTime + preDashDelay);
            bool alreadyAttacked = pawn.UsedCurrentSkill();
            return isTesting && !alreadyAttacked;
        }

        public override IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
        {
            yield return timeUntilStartsTesting + preDashDelay;
            yield return Mathf.Max(preTime - timeUntilStartsTesting, 0f);
            yield return animationTime + postTime;
        }
    }

    


}