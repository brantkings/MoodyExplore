using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[CreateAssetMenu(menuName = "Pixel Art Camera/Screen Effect Gradient", fileName = "PA_GradientEffect_")]
public class PixelArtLookUpSettingsGradientChange : ScriptableObject
{
    public Gradient gradientAdd;
    public Gradient gradientMult;
    public float duration;
    public bool forceDuration;

    public void DoEffect(PixelArtLookUpSettingsData data, float newDuration)
    {
        float d = forceDuration ? duration : newDuration;
        DoGradient(gradientAdd, d, 0f, 1f, (x) => data.addColor = x);
        DoGradient(gradientMult, d, 0f, 1f, (x) => data.multiplyColor = x);
    }

    public void DoEffectInverted(PixelArtLookUpSettingsData data, float newDuration)
    {
        float d = forceDuration ? duration : newDuration;
        DoGradient(gradientAdd, d, 1f, 0f, (x) => data.addColor = x);
        DoGradient(gradientMult, d, 1f, 0f, (x) => data.multiplyColor = x);
    }


    private delegate void ApplyColor(Color a);

    private Tween DoGradient(Gradient grad, float duration, float from, float to, ApplyColor apply)
    {
        float a = from;
        return DOTween.To(() => a, (x) =>
        {
            apply(grad.Evaluate(x));
            a = x;
        }, to, duration).OnKill(()=> apply(grad.Evaluate(to))).SetId(this);
    }
}
