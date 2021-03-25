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

    public Color defaultProgressColor = Color.yellow;

    public Text text;

    public override bool CanShowSkill(MoodSkill skill)
    {
        return true;
    }

    public override void Hide(MoodPawn pawn)
    {
        mainObject.gameObject.SetActive(false);
    }

    public override void ShowSkill(MoodPawn pawn, MoodSkill skill)
    {
        Debug.LogErrorFormat("Barking {0} {1}", pawn, skill);
        text.color = skill.GetColor();
        text.text = skill.GetName();
        mainObject.gameObject.SetActive(true);
    }

    public override IEnumerator ShowSkillLive(MoodPawn pawn, MoodSkill skill, Vector3 directionUsed)
    {
        Hide(pawn);
        ShowSkill(pawn, skill);
        while (true)
        {
            skill.GetProgress(pawn.GetTimeElapsedSinceBeganCurrentSkill(), out float progress, out Color? progressColor);
            Debug.LogErrorFormat("Bark -> progress for {0} is (prog:{1} * sizx:{3} = {4}) because of time now is {2}.", skill, progress, pawn.GetTimeElapsedSinceBeganCurrentSkill(), mainObject.sizeDelta.x, mainObject.sizeDelta.x * progress);
            progressObject.anchoredPosition = new Vector3(mainObject.rect.width * progress, progressObject.anchoredPosition.y);
            progressObjectRenderer.color = progressColor.HasValue ? progressColor.Value : defaultProgressColor;
            if (pawn.GetCurrentSkill() != skill)
            {
                break;
            }
            yield return null;
        }
        Hide(pawn);
    }
}
