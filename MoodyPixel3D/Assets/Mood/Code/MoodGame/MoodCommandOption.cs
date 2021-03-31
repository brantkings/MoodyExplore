using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MoodCommandOption : MonoBehaviour
{
    private Text _descriptorText;
    private string _description;
    public Text text;
    public Outline outline;
    public Image stanceChange;
    public Image stanceCancel;
    public Transform focusCostGroup;

    public Color unselectedOutlineColor;
    public Color possibleColor = Color.white;
    public Color impossibleColor = Color.gray;
    public Color costColor = Color.gray;
    public Color costNumberColor = Color.gray;

    public int costSize = 24;
    public Gradient selectedOutlineColorAnimation;
    public float selectedColorAnimationDuration;

    private Tween _selectedAnim;

    private MoodSkill _skill;
    
    public struct Parameters
    {
        public string optionName;
    }

    private void Awake()
    {
        _descriptorText = GetComponentInParent<MoodCommandController>().GetDescriptorText();
    }

    public void SetOption(MoodSkill skill)
    {
        _skill = skill;
        StaminaCostMoodSkill stamina = skill as StaminaCostMoodSkill;
        //Debug.Log($"{costColor} = {costColor.ToHexStringRGB()}");
        //text.text = $"<color=#{_skill.GetColor().ToHexStringRGB()}>{_skill.GetName()}</color>" +  (stamina != null && stamina.GetStaminaCost() != 0f? $"<size={costSize}><color=#{costNumberColor.ToHexStringRGB()}> {stamina.GetStaminaCost()}</color><color=#{costColor.ToHexStringRGB()}>SP</color></size>" : string.Empty);
        text.text = $"{_skill.GetName()}" +  (stamina != null && stamina.GetStaminaCost() != 0f? $"<size={costSize}><color=#{costNumberColor.ToHexStringRGB()}> {stamina.GetStaminaCost()}</color><color=#{costColor.ToHexStringRGB()}>SP</color></size>" : string.Empty);
        text.enabled = true;
        _description = skill.GetDescription();

        SetFocusCost(skill.GetFocusCost());

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

    private void SetFocusCost(int amountFocusCost)
    {
        if (amountFocusCost <= 0)
        {
            focusCostGroup.gameObject.SetActive(false);
        }

        int childId = 0;

        foreach(Transform child in focusCostGroup)
        {
            child.gameObject.SetActive(childId < amountFocusCost);
            childId++;
        }
    }

    public void SetSelected(bool set)
    {
        if (set)
        {
            outline.effectColor = selectedOutlineColorAnimation.Evaluate(0f);
            SetColorGradient(0f);
            _selectedAnim = DOTween.To(GetColorGradient, SetColorGradient, 1f, selectedColorAnimationDuration).SetEase(Ease.Linear).SetUpdate(true).SetLoops(-1);
            if (_descriptorText != null)
                _descriptorText.text = _description;
        }
        else
        {
            _selectedAnim.KillIfActive(true);
            _selectedAnim = null;

            SetOutlineColor(unselectedOutlineColor);
        }
    }

    public void SetPossible(bool possible, MoodSkill skill)
    {
        text.color = possible ? skill.GetColor() : impossibleColor;
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
