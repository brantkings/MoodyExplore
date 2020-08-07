using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IAnimatorEvent
{
    int Id { get; }
    void EventFromAnimator();
}

[System.Serializable]
public struct AnimatorID
{
    [SerializeField]
    private string id;

    private int _numericalId;
    private bool _got;
    public int GetNumericalID()
    {
        if(_got == false)
        {
            if (_numericalId == 0) _numericalId = Animator.StringToHash(id);
        }
        _got = true;
        return +_numericalId;
    }

    public bool Equals(AnimatorID id)
    {
        return GetNumericalID() == id.GetNumericalID();
    }
}


public class AnimatorEvent : MonoBehaviour, IAnimatorEvent {

    public AnimatorID id;

    public UnityEngine.Events.UnityEvent call;

    public int Id
    {
        get
        {
            return id.GetNumericalID();
        }
    }

    public void EventFromAnimator()
    {
        call.Invoke();
    }
}
