using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangeSkillBark : RangeShow
{
    public RectTransform mainObject;
    public RectTransform progressObject;
    public Image progressObjectRenderer;
    public Animator anim;

    public Color defaultProgressColor = Color.yellow;

    public Text text;

    private MoodPawn pawn;

    private MoodSkill _interrupted;

    private void Awake()
    {
        pawn = GetComponentInParent<MoodPawn>();
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

    public void TrueHide()
    {
        anim.SetBool("Show", false);
        mainObject.gameObject.SetActive(false);
    }

    public override void Hide(MoodPawn pawn)
    {
        anim.SetBool("Show", false);
    }

    public override void ShowSkill(MoodPawn pawn, MoodSkill skill)
    {
        Debug.LogErrorFormat("Barking {0} {1}", pawn, skill);
        text.color = skill.GetColor();
        text.text = skill.GetName();
        anim.SetBool("Show", true);
        mainObject.gameObject.SetActive(true);
    }

    public override IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill, Vector3 directionUsed)
    {
        Debug.LogErrorFormat("[BARK] Hide because begin {0} [{1}]", skill, Time.frameCount);
        TrueHide();

        Debug.LogErrorFormat("[BARK] Show because begin {0}. Current skill is {1} [{2}]", skill, pawn.GetCurrentSkill(), Time.frameCount);
        ShowSkill(pawn, skill);
        while (pawn.GetCurrentSkill() == skill) 
        {
            skill.GetProgress(pawn, directionUsed, pawn.GetTimeElapsedSinceBeganCurrentSkill(), out float progress, out Color? progressColor);
            //Debug.LogErrorFormat("Bark -> progress for {0} is (prog:{1} * sizx:{3} = {4}) because of time now is {2}.", skill, progress, pawn.GetTimeElapsedSinceBeganCurrentSkill(), mainObject.sizeDelta.x, mainObject.sizeDelta.x * progress);
            progressObject.anchoredPosition = new Vector3(mainObject.rect.width * progress, progressObject.anchoredPosition.y);
            progressObjectRenderer.color = progressColor.HasValue ? progressColor.Value : defaultProgressColor;
            yield return null;
            if(_interrupted == skill)
            {
                if(pawn.GetCurrentSkill() == null)
                {
                    Debug.LogErrorFormat("[BARK] Hide because interrupted {0} and current skill is {1} [{2}]", skill, pawn.GetCurrentSkill(), Time.frameCount);
                    TrueHide();
                }
                else
                {
                    anim.SetTrigger("Interrupt");
                }
                _interrupted = null;
                yield break;
            }
        }
        Debug.LogErrorFormat("[BARK] Hide because end {0} [{1}]", skill, Time.frameCount);
        if(!pawn.IsExecutingSkill()) Hide(pawn);
    }

    private void OnInterruptSkill(MoodPawn pawn, MoodSkill skill)
    {
        _interrupted = skill;
    }
}
