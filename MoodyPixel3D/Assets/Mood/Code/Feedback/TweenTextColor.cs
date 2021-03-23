using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenTextColor : TweenGradientColor<Text>
{
    public override void SetValue(Color value)
    {
        Addon.color = value;
    }


}
