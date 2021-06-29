using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtFocusable : MonoBehaviour
{
    public int focusablePriority;

    public LHH.Switchable.Switchable maximizedSwitchable;
    public LHH.Switchable.Switchable selectedSwitchable;
    public LHH.Switchable.Switchable focusedSwitchable;

    private Thought thought;
    public Text[] thoughtText;
    public Image[] thoughtIcon;
    
    


    #region Interface
    public virtual ThoughtFocusable Create(Thought t)
    {
        this.thought = t;
        foreach(Text txt in thoughtText)
        { 
            txt.text = $"\"{t.thoughtPhrase}\"";
            txt.color = t.thoughtPhraseColor;
        }
        foreach (Image i in thoughtIcon)
        {
            i.sprite = t.thoughtIcon;
            i.color = t.thoughtIconColor;
        }
        return this;
    }

    public void SetMaximize(bool set)
    {
        maximizedSwitchable?.Set(set);
    }

    public void SetHovered(bool set)
    {
        selectedSwitchable?.Set(set);
    }

    public bool CanSetFocus(MoodPawn pawn)
    {
        return true;
    }

    public bool IsFocused()
    {
        return focusedSwitchable != null? focusedSwitchable.IsOn() : false;
    }

    public void SetFocused(bool set, MoodPawn pawn)
    {
        focusedSwitchable?.Set(set);
    }

    /// <summary>
    /// Toggle the focus and returns wether or not it was focused or not.
    /// </summary>
    /// <param name="pawn"></param>
    /// <returns></returns>
    public bool ToggleFocus(MoodPawn pawn)
    {
        if(IsFocused())
        {
            SetFocused(false, pawn);
            return false;
        }
        else
        {
            SetFocused(true, pawn);
            return true;
        }
    }

    public Thought GetThought()
    {
        return thought;
    }
    #endregion
}
