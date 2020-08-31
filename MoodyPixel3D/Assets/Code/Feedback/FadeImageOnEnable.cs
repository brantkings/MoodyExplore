using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class FadeImageOnEnable : TweenOnEnable<Image, float>
{
    public float maxFade = 1f;
    
    protected override Tween ExecuteTweenItself(float to, float duration)
    {
        return Addon.DOFade(to, duration);
    }

    protected override float GetInValue()
    {
        return maxFade;
    }

    protected override float GetOutValue()
    {
        return 0f;
    }

    public override void SetValue(float value)
    {
        Color c = Addon.color;
        c.a = value;
        Addon.color = c;
    }


}
