using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorOnEnable : GradientColorTween<SpriteRenderer>
{
    public override void SetValue(Color value)
    {
        Addon.color = value;
    }

    
}
