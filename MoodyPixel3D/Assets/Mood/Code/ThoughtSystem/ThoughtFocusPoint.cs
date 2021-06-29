using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThoughtFocusPoint : MonoBehaviour
{
    [SerializeField]
    private LHH.Switchable.Switchable switchableUsed;

    [SerializeField]
    private RectTransform objectToMove;

    public RectTransform GetObjectToMove()
    {
        return objectToMove;
    }

    public void SetUsed(bool set)
    {
        switchableUsed?.Set(set);
    }
}
