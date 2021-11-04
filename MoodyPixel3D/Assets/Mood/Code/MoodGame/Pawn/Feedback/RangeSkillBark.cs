using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LHH.ScriptableObjects.Events;

public class RangeSkillBark : RangeShow
{
    public RectTransform mainObject;
    public RectTransform progressObject;
    public Color defaultColor = Color.white;
    public float barksShownEachEveryNBeat = 1f;
    public Image progressObjectRenderer;
    public bool continuous;
    public Ease nonContinuousEase;
    [Range(0f,1f)]
    public float beatMultiplierDelayEase;
    public RectTransform timeDivider;
    public Animator anim;
    public MoodSkill notPerceivedSkill;

    public Color defaultProgressIncreasingColor = Color.yellow;
    public Color defaultProgressDecreasingColor = Color.magenta;

    public Text text;

    private MoodPawn pawn;

    private MoodSkill _interrupted;

    [Header("Feedback")]
    public ScriptableEvent[] beginSkill;
    public ScriptableEvent[] endSkill;
    public ScriptableEvent[] interruptSkill;
    public ScriptableEvent[] cancelSkill;

    private void Awake()
    {
        pawn = GetComponentInParent<MoodPawn>();
        timeDivider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        pawn.OnInterruptSkill += OnInterruptSkill;
    }

    private void OnDisable()
    {
        pawn.OnInterruptSkill -= OnInterruptSkill;
    }

    public override bool CanShowSkill(MoodSkill skill)
    {
        return skill.ShowsBark();
    }

    private bool IsShowing()
    {
        return mainObject.gameObject.activeSelf;
    }

    public void TrueHide()
    {
        //Debug.LogErrorFormat("TRUE HIDE {0}, text was {1} [{2}]", pawn, text.text, Time.frameCount);
        anim.SetBool("Show", false);
        mainObject.gameObject.SetActive(false);
    }

    public override void Hide(MoodPawn pawn)
    {
        //Debug.LogErrorFormat("Hide {0}, text was {1} [{2}]", pawn, text.text, Time.frameCount);
        anim.SetBool("Show", false);
    }

    public override void ShowSkill(MoodPawn pawn, MoodSkill skill)
    {
        //Debug.LogErrorFormat("Barking for '{0}' with '{1}'! [{2}]", pawn.name, skill.name, Time.frameCount);
        text.color = skill.GetColor().HasValue ? skill.GetColor().Value : defaultColor;
        text.text = skill.GetName(pawn);
        anim.SetBool("Show", true);
        mainObject.gameObject.SetActive(true);
    }

    private IEnumerator<RectTransform> CreateTimePointers()
    {
        //yield return timeDivider;
        foreach (Transform t in timeDivider.parent)
        {
            RectTransform r = t as RectTransform;
            if(r != null)
            {
                r.gameObject.SetActive(true);
                yield return r;
            }
        }
        while(true)
        {
            yield return Instantiate(timeDivider, timeDivider.parent);
        }
    }

    private void DivideTime(float totalTime, float marksPerBeat = 1f)
    {
        float beats = TimeBeatManager.GetNumberOfBeats(totalTime) / marksPerBeat;
        float beatXDistance = TimeBeatManager.GetBeatLength() * marksPerBeat / totalTime;
        float beatXNow = 0f;
        int numberOfBeats = 0;
        IEnumerator<RectTransform> rects = CreateTimePointers();
        while(beats >= 0f)
        {
            rects.MoveNext();
            RectTransform newR = rects.Current;
            newR.anchorMin = new Vector2(beatXNow, newR.anchorMin.y);
            newR.anchorMax = new Vector2(beatXNow, newR.anchorMax.y);
            newR.anchoredPosition = new Vector2(0f, newR.anchoredPosition.y);
            beatXNow += beatXDistance;
            numberOfBeats++;
            beats--;
        }

        //Debug.LogErrorFormat("[BARK DIVIDE] Created {0} divides for {1} (/ {2} = {3}).", numberOfBeats, totalTime, TimeBeatManager.GetBeatLength(), totalTime / TimeBeatManager.GetBeatLength());
        foreach (Transform t in timeDivider.parent)
        {
            RectTransform r = t as RectTransform;
            if(r != null)
            {
                numberOfBeats--;
                if (numberOfBeats < 0) r.gameObject.SetActive(false);
            }
        }
    }

    private float GetTotalTime(MoodPawn pawn, MoodSkill skill, Vector3 direction)
    {
        float totalTime = 0f;
        foreach (float time in skill.GetTimeIntervals(pawn, direction))
        {
            totalTime += time;
        }
        return totalTime;
    }

    public override IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill, Vector3 directionUsed)
    {
        //Debug.LogErrorFormat("[BARK] Hide because begin {0} [{1}]", skill, Time.frameCount);
        bool wasHiding = !IsShowing();
        if(wasHiding)
        {
            JustBeganBark();
        }
        TrueHide();

        //Debug.LogErrorFormat("[BARK] Show because begin {0}. Current skill is {1} [{2}]", skill, pawn.GetCurrentSkill(), Time.frameCount);

        if(!MoodPlayerController.Instance.Pawn.IsSensing(pawn))
        {
            skill = notPerceivedSkill;
        }

        ShowSkill(pawn, skill);
        JustBeganSkill();
        float totalTimeNow = 0f;
        int beatBefore = 0;
        float progressPercentageBefore = 0f;

        DOTween.Kill(this);

        progressObject.anchoredPosition = new Vector2(0f, progressObject.anchoredPosition.y);

        if(pawn.GetCurrentSkill() == skill)
        {

            while (pawn.GetCurrentSkill() == skill) 
            {
                float timeSinceSkill = pawn.GetTimeElapsedSinceBeganCurrentSkill();
                int currentBeat = Mathf.FloorToInt(TimeBeatManager.GetNumberOfBeats(timeSinceSkill)) + 1;
                float totalTime, progressPercentage; Color? progressColor;
                if (continuous)
                {
                    skill.GetProgress(pawn, directionUsed, timeSinceSkill, out totalTime, out progressPercentage, out progressColor);
                }
                else
                {
                    skill.GetProgress(pawn, directionUsed, TimeBeatManager.GetTime(currentBeat), out totalTime, out progressPercentage, out progressColor);
                }
                if(totalTime != totalTimeNow)
                {
                    totalTimeNow = totalTime;
                    DivideTime(totalTimeNow, barksShownEachEveryNBeat);
                }
                if(continuous)
                {
                    progressObject.anchoredPosition = new Vector2(mainObject.rect.width * progressPercentage, progressObject.anchoredPosition.y);
                    progressObjectRenderer.color = progressColor.HasValue ? progressColor.Value : ((progressPercentage >= progressPercentageBefore) ? defaultProgressIncreasingColor : defaultProgressDecreasingColor);
                    progressPercentageBefore = progressPercentage;
                    if (progressPercentage == 1f && progressPercentageBefore != 1f) JustExecutedBark();
                }
                else if(currentBeat != beatBefore) //If it is beat by beat, with tweens
                {
                    progressObject.DOAnchorPos(
                        new Vector2(mainObject.rect.width * progressPercentage, progressObject.anchoredPosition.y),
                        TimeBeatManager.GetBeatLength() * (1f - beatMultiplierDelayEase))
                        .SetEase(nonContinuousEase)
                        .SetId(this)
                        .SetDelay(TimeBeatManager.GetBeatLength() * beatMultiplierDelayEase).OnKill(() =>
                        {
                            //Debug.LogFormat("now{0} before{1} {2}", progressPercentage, progressPercentageBefore, skill);
                            if (progressPercentage == 1f) JustExecutedBark();
                        });
                    progressObjectRenderer.color = progressColor.HasValue ? progressColor.Value : ((progressPercentage >= progressPercentageBefore) ? defaultProgressIncreasingColor : defaultProgressDecreasingColor);
                    progressPercentageBefore = progressPercentage;
                }
            
                beatBefore = currentBeat;
                yield return null;
                if(_interrupted == skill)
                {
                    if(pawn.GetCurrentSkill() == null)
                    {
                        //Debug.LogErrorFormat("[BARK] Hide because interrupted {0} and current skill is {1} [{2}]", skill, pawn.GetCurrentSkill(), Time.frameCount);
                        JustInterruptedSkillIntoNothing();
                    }
                    else
                    {
                        JustInterruptedSkillIntoOtherSkill();
                    }
                    _interrupted = null;
                    yield break;
                }
            }
        }
        else
        {
            yield return null; //Let one frame pass so the animator leaves the off state.
            Hide(pawn);
        }
        //Debug.LogErrorFormat("[BARK] Hide because end {0} [{1}]", skill, Time.frameCount);
        if (!pawn.IsExecutingSkill())
        {
            Hide(pawn);
            JustEndedBark();
        }
        JustEndedSkill();
    }

    private void JustBeganBark()
    {
        beginSkill.Invoke(transform);
    }

    private void JustBeganSkill()
    {

    }

    private void JustEndedSkill()
    {

    }

    private void JustEndedBark()
    {
        endSkill.Invoke(transform);
    }

    private void JustExecutedBark()
    {
        anim.SetTrigger("Execute");
    }

    private void JustInterruptedSkillIntoOtherSkill()
    {
        anim.SetTrigger("Interrupt");
        interruptSkill.Invoke(transform);
    }

    private void JustInterruptedSkillIntoNothing()
    {
        anim.SetTrigger("Cancel");
        cancelSkill.Invoke(transform);
    }

    private void OnInterruptSkill(MoodPawn pawn, MoodSkill skill)
    {
        _interrupted = skill;
    }
}
