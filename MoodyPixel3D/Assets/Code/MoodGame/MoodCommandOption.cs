using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MoodCommandOption : MonoBehaviour
{
    public Text text;
    public Outline outline;
    public Image stanceChange;
    public Image stanceCancel;

    public Color unselectedOutlineColor;
    public Color possibleColor = Color.white;
    public Color impossibleColor = Color.gray;
    public Color costColor = Color.gray;

    public int costSize = 24;
    public Gradient selectedOutlineColorAnimation;
    public float selectedColorAnimationDuration;

    private Tween _selectedAnim;

    private MoodSkill _skill;
    
    public struct Parameters
    {
        public string optionName;
    }
    
    public void SetOption(MoodSkill skill)
    {
        _skill = skill;
        StaminaCostMoodSkill stamina = skill as StaminaCostMoodSkill;
        Debug.Log($"{costColor} = {costColor.ToHexStringRGB()}");
        text.text = _skill.GetName() +  (stamina != null && stamina.GetStaminaCost() != 0f? $"<size={costSize}><color=#{costColor.ToHexStringRGB()}> {stamina.GetStaminaCost()}SP</color></size>" : string.Empty);
        text.enabled = true;

        bool didChangeStance = false;
        foreach(MoodStance stance in skill.GetStancesThatWillBeAdded())
        {
            SetStance(stanceChange, stanceCancel, stance.GetIcon(), false);
            didChangeStance = true;
            break;
        }
        if(!didChangeStance)
        {
            foreach (MoodStance stance in skill.GetStancesThatWillBeRemoved())
            {
                SetStance(stanceChange, stanceCancel, stance.GetIcon(), true);
                didChangeStance = true;
                break;
            }
        }

        if (!didChangeStance) SetStance(stanceChange, stanceCancel, null);
    }

    private void SetStance(Image image, Image cancelImage, Sprite spr = null, bool setCancel = false)
    {
        if(spr != null)
        {
            image.sprite = spr;
            image.gameObject.SetActive(true);
            cancelImage.gameObject.SetActive(setCancel);
        }
        else
        {
            image.gameObject.SetActive(false);
            cancelImage.gameObject.SetActive(false);
        }
    }

    public void SetSelected(bool set)
    {
        if (set)
        {
            outline.effectColor = selectedOutlineColorAnimation.Evaluate(0f);
            SetColorGradient(0f);
            _selectedAnim = DOTween.To(GetColorGradient, SetColorGradient, 1f, selectedColorAnimationDuration).SetEase(Ease.Linear).SetUpdate(true).SetLoops(-1);
        }
        else
        {
            _selectedAnim.KillIfActive(true);
            _selectedAnim = null;

            SetOutlineColor(unselectedOutlineColor);
        }
    }

    public void SetPossible(bool possible)
    {
        text.color = possible ? possibleColor : impossibleColor;
    }

    private float _colorGradientAnimPos;

    private void SetOutlineColor(Color c)
    {
        outline.effectColor = c;
    }
    
    private void SetColorGradient(float t)
    {
        _colorGradientAnimPos = t;
        SetOutlineColor(selectedOutlineColorAnimation.Evaluate(t));
    }

    private float GetColorGradient()
    {
        return _colorGradientAnimPos;
    }
    
}
