using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Switchable
{
    public class SwitchableCanvasGroup : SwitchableAddon
    {
        [SerializeField]
        CanvasGroup[] sideOn;

        [SerializeField]
        CanvasGroup[] sideOff;

        [Range(0f,1f)]
        public float minAlpha = 0f;
        [Range(0f, 1f)]
        public float maxAlpha = 1f;
        private float _count;
        public bool unscaled;
        public bool deactivateAfter;
        public float defaultDuration = 0.5f;

        public AnimationCurve alphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public IEnumerator TurnRoutine(bool on, float duration)
        {
            //Debug.LogFormat(this, "Hey {0} am turning {1} with duration {2}", this, on, duration);
            float timeCount = 0f;
            float alphaMidValue = 0f;
            while (timeCount < duration)
            {
                float timeDelta = GetDeltaTime();
                alphaMidValue += (on ? 1f : -1f) * (timeDelta / duration);
                float alphaValue = GetAlphaValue(alphaMidValue, minAlpha, maxAlpha);
                foreach (CanvasGroup c in sideOn) c.alpha = GetAlphaValue(alphaMidValue, minAlpha, maxAlpha);
                foreach (CanvasGroup c in sideOff) c.alpha = GetAlphaValue(alphaMidValue, maxAlpha, minAlpha);
                yield return Wait(timeDelta);
                timeCount += timeDelta;
            }

            alphaMidValue = on ? 1f : 0f;

            foreach (CanvasGroup c in sideOn)
            {
                c.alpha = GetAlphaValue(alphaMidValue, minAlpha, maxAlpha);
                if (deactivateAfter) c.gameObject.SetActive(on);
                //Debug.LogFormat(this, "Setting {0} alpha as {1}", c, GetAlphaValue(alphaMidValue, minAlpha, maxAlpha));
            }
            foreach (CanvasGroup c in sideOff)
            {
                c.alpha = GetAlphaValue(alphaMidValue, maxAlpha, minAlpha);
                if (deactivateAfter) c.gameObject.SetActive(!on);
            }
        }

        private float GetAlphaValue(float x, float alphaMin, float alphaMax)
        {
            //Debug.LogFormat("Value for {0} is {1} -> {2} ({3})", x, alphaCurve.Evaluate(x), Mathf.Lerp(alphaMin, alphaMax, alphaCurve.Evaluate(x)), name);
            return Mathf.Lerp(alphaMin, alphaMax, alphaCurve.Evaluate(x));
        }

        private float GetDeltaTime()
        {
            return unscaled? Time.unscaledDeltaTime : Time.deltaTime;
        }

        private IEnumerator Wait(float time)
        {
            if (unscaled)
                yield return new WaitForSecondsRealtime(time);
            else yield return new WaitForSeconds(time);
        }

        public override IEnumerator SwitchSet(bool on, ISwitchableAddon.DelSwitchableAddonEvent onFinish = null)
        {
            yield return TurnRoutine(on, defaultDuration);
        }

        public override void SwitchSetImmediate(bool on)
        {
            StartCoroutine(TurnRoutine(on, 0f));
        }
    }
}
