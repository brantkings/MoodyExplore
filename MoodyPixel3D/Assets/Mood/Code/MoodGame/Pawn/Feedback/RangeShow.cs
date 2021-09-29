using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public interface IRangeShow
{
    void ShowSkill(MoodPawn pawn, MoodSkill skill);
    void Hide(MoodPawn pawn);
}

public interface IRangeShowLive : IRangeShow
{
    IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill, Vector3 skillDirection);
}

public interface IRangeShowDirected
{
    void SetDirection(MoodPawn pawn, MoodSkill skill, Vector3 sanitizedDirectionLength);
}

public abstract class RangeShow : MonoBehaviour, IRangeShowLive
{
    [System.Serializable]
    public struct SkillDirectionSanitizer
    {
        public MoodSkill.DirectionFixer fixer;
        public float minLength;
        public float maxLength;

        public SkillDirectionSanitizer(float max)
        {
            minLength = 0f; maxLength = max;
            fixer = MoodSkill.DirectionFixer.LetAll;
        }

        public SkillDirectionSanitizer(float min, float max)
        {
            minLength = min; maxLength = max;
            fixer = MoodSkill.DirectionFixer.LetAll;
        }

        public SkillDirectionSanitizer(float min, float max, MoodSkill.DirectionFixer fixer)
        {
            minLength = min; maxLength = max;
            this.fixer = fixer;
        }

        public SkillDirectionSanitizer(float min, float max, float maxAngle = 180f, float maxConeAngle = 0f)
        {
            minLength = min; maxLength = max;
            fixer = new MoodSkill.DirectionFixer()
            {
                angleFromForward = maxAngle,
                coneAngle = maxConeAngle,
                mirrored = false
            };
        }

        public Vector3 Sanitize(Vector3 direction, Vector3 pawnDirection)
        {
            float mag = direction.magnitude;
            return fixer.Sanitize(direction, pawnDirection).normalized * Mathf.Clamp(mag, minLength, maxLength);
        }

        public static SkillDirectionSanitizer DefaultValue
        {
            get
            {
                return new SkillDirectionSanitizer()
                {
                    fixer = MoodSkill.DirectionFixer.LetAll,
                    minLength = 0f,
                    maxLength = float.PositiveInfinity
                };
            }
        }

        public override string ToString()
        {
            return string.Format("[SkillDirS:{0}-{1} with {2}]", minLength, maxLength, fixer);
        }
    }
    public abstract void ShowSkill(MoodPawn pawn, MoodSkill skill);
    public abstract void Hide(MoodPawn pawn);
    public abstract IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill, Vector3 directionUsed);

    public abstract bool CanShowSkill(MoodSkill skill);
}

public abstract class RangeShow<T> : RangeShow
{
    

    public interface IRangeShowPropertyGiver
    {
        T GetRangeProperty();
    }

    public interface IRangeShowLivePropertyGiver
    {
        bool ShouldShowNow(MoodPawn pawn);

    }

    public abstract void Show(MoodPawn pawn, T property);

    public override bool CanShowSkill(MoodSkill skill)
    {
        return skill.ImplementsRangeShow<T>();
    }

    public override void ShowSkill(MoodPawn pawn, MoodSkill skill)
    {
        //Debug.LogFormat("Does {0} implements {1}? {2}. Is it IRangeShowPropertyGiver? {3}", skill, typeof(T), skill.ImplementsRangeShow<T>(), skill is IRangeShowPropertyGiver);
        if (CanShowSkill(skill))
        {
            Show(pawn, skill.GetRangeShowProperty<T>().GetRangeProperty());
        }
        else
        {
            Hide(pawn);
        }
    }

    public override IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill, Vector3 directionUsed)
    {
        IRangeShowLivePropertyGiver isLive = skill as IRangeShowLivePropertyGiver;
        if(isLive != null)
        {
            yield return CheckSkillRoutineLive(pawn, skill, directionUsed, this as IRangeShowDirected, isLive, skill as IRangeShowPropertyGiver);
        }
    }
    
    protected Vector3 GetPreviewDirection(MoodSkill skill, Vector3 direction)
    {
        return direction;
    }

    private IEnumerator CheckSkillRoutineLive(MoodPawn pawn, MoodSkill skill, Vector3 skillDirection, IRangeShowDirected directed, RangeShow<T>.IRangeShowLivePropertyGiver isLive, RangeShow<T>.IRangeShowPropertyGiver propertyGetter)
    {
        bool repeat = true;
        bool wasShowing = false;
        while (repeat && pawn.GetCurrentSkill() == skill)
        {
            bool shouldShow = isLive.ShouldShowNow(pawn);
            if(shouldShow != wasShowing)
            {
                if (shouldShow)
                {
                    Show(pawn, propertyGetter.GetRangeProperty());
                }
                else
                {
                    Hide(pawn);
                }
                wasShowing = shouldShow;
            }
            if(!directed.Equals(null))
                directed.SetDirection(pawn, skill, skillDirection);
            yield return null;
            //Debug.LogFormat("[RANGESHOW] {0} is still showing {1} with {2}. Its current skill is {3}", pawn.name, skill.name, propertyGetter, pawn.GetCurrentSkill());
        }
        Hide(pawn);
    }
}
