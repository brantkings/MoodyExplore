using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Switchable
{
    public class SwitchableCanvasGroup : SwitchableEffect
    {
        [SerializeField]
        CanvasGroup[] sideOn;

        [SerializeField]
        CanvasGroup[] sideOff;

        public bool unscaled;
        public bool deactivateAfter;
        public float defaultDuration = 0.5f;

        public IEnumerator TurnRoutine(bool on, float duration)
        {
            float count = 0f;
            while (count < duration)
            {
                float timeDelta = GetDeltaTime();
                float amount = timeDelta / duration;
                foreach (CanvasGroup c in sideOn) c.alpha += (on? 1f : -1f) * amount;
                foreach (CanvasGroup c in sideOff) c.alpha += (on? -1f : 1f) * amount;
                yield return Wait(timeDelta);
            }

            foreach (CanvasGroup c in sideOn)
            {
                c.alpha = (on ? 1f : 0f);
                if (deactivateAfter) c.gameObject.SetActive(on);
            }
            foreach (CanvasGroup c in sideOff)
            {
                c.alpha = (on ? 0f : 1f);
                if (deactivateAfter) c.gameObject.SetActive(!on);
            }
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

        protected override void Effect(bool on)
        {
            StopAllCoroutines();
            StartCoroutine(TurnRoutine(on, defaultDuration));
        }
    }
}
