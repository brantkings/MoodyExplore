using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDObject : MonoBehaviour
{
    public Image bg;
    public Image fill;

    private bool? _damaged;

    public void SetDamaged(bool set, bool feedback = true)
    {
        Debug.LogFormat("Setting {0} {1}. Current is {2}. Tests: {3} {4}", this, set, _damaged, set != _damaged, _damaged.HasValue && _damaged.Value != set);
        if(set != _damaged)
        {
            _damaged = set;
            SetDamageFeedbackBegin(set);
            if (feedback)
            {
                if (_damaged == true) TakeDamageFeedback();
                else HealFeedback();
            }
            else
            {
                SetDamageFeedbackComplete(set);
            }
        }
    }

    private void SetDamageFeedbackBegin(bool damaged)
    {
        fill.gameObject.SetActive(true);
    }

    private void SetDamageFeedbackComplete(bool damaged)
    {
        fill.gameObject.SetActive(!damaged);
    }

    private void TakeDamageFeedback()
    {
        SetDamageFeedbackComplete(true);
    }

    private void HealFeedback()
    {
        SetDamageFeedbackComplete(false);
    }
}
