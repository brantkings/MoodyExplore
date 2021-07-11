using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtFocusPoint : MonoBehaviour
{
    [SerializeField]
    private LHH.Switchable.Switchable switchableUsed;

    [SerializeField]
    private RectTransform objectToMove;

    [SerializeField]
    private Text experienceText;

    public RectTransform GetObjectToMove()
    {
        return objectToMove;
    }

    public void SetUsed(bool set)
    {
        switchableUsed?.Set(set);
    }

    public void SetExperienceText(int experienceNow, int experienceTotal)
    {
        experienceText.text = $"{experienceNow}/{experienceTotal}";
    }

    public void UnsetExperienceText()
    {
        experienceText.text = "";
    }
}
