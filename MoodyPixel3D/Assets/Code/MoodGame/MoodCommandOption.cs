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

    public Color unselectedOutlineColor;
    public Color possibleColor = Color.white;
    public Color impossibleColor = Color.gray;
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
        text.text = _skill.GetName();
        text.enabled = true;
    }

    public void SetSelected(bool set)
    {
        if (set)
        {
            outline.effectColor = selectedOutlineColorAnimation.Evaluate(0f);
            SetColorGradient(0f);
            _selectedAnim = DOTween.To(GetColorGradient, SetColorGradient, 1f, selectedColorAnimationDuration).SetEase(Ease.Linear).SetLoops(-1);
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
