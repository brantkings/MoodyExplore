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

    [Space]
    public Animator anim;
    public AnimatorID executeTrigger = "Execute";
    public AnimatorID confirmTrigger = "Select";
    public AnimatorID bufferBool = "Buffer";

    [Space()]
    public Color unselectedOutlineColor;
    public Color possibleColor = Color.white;
    public Color impossibleColor = Color.gray;
    public Color costColor = Color.gray;
    public Color costNumberColor = Color.gray;

    public int costSize = 24;
    public Gradient selectedOutlineColorAnimation;
    public float selectedColorAnimationDuration;

    private Tween _selectedAnim;
    
    public struct Parameters
    {
        public string optionName;
    }

    private void Awake()
    {
        _descriptorText = GetComponentInParent<MoodCommandController>().GetDescriptorText();
    }

    public void SetText(string name, string description, float staminaCost)
    {
        text.text = $"{name}" + (staminaCost != 0f ? $"<size={costSize}><color=#{costNumberColor.ToHexStringRGB()}> {staminaCost}</color><color=#{costColor.ToHexStringRGB()}>SP</color></size>" : string.Empty);
        text.enabled = true;
        _description = description;
    }

    public void SetStancePreview(Sprite spr = null, bool setCancel = false)
    {
        SetStancePreview(stanceChange, stanceCancel, spr, setCancel);
    }

    private void SetStancePreview(Image image, Image cancelImage, Sprite spr = null, bool setCancel = false)
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

    public void SetFocusCost(int amountFocusCost)
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

    public void SetPossible(bool possible, IMoodSelectable skill)
    {
        text.color = possible ? (skill.GetColor().HasValue? skill.GetColor().Value : possibleColor) : impossibleColor;
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

    public void FeedbackBuffer(bool set)
    {
        anim.SetBool(bufferBool, set);
    }

    public void FeedbackExecute()
    {
        anim.SetTrigger(executeTrigger);
    }
    public void FeedbackConfirmSelection()
    {
        anim.SetTrigger(confirmTrigger);
    }

}
