using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public static class TweenUtils
{
    public static void KillIfActive(this Tween tween, bool complete = false)
    {
        if (tween != null && tween.IsActive()) tween.Kill(complete);
    }

    public static void CompleteIfActive(this Tween tween, bool withCallbacks = true)
    {
        if (tween != null && tween.IsActive()) tween.Complete(withCallbacks);
    }
}
