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
public class AnimatorID
{
    [SerializeField]
    private string id;

    private int _numericalId;
    public int GetNumericalID()
    {
        if (_numericalId == 0)
        {
            _numericalId = Animator.StringToHash(id);
        }
        return _numericalId;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(id);
    }

    /*public bool Equals(AnimatorID id)
    {
        return GetNumericalID() == id.GetNumericalID();
    }*/

    public static implicit operator int(AnimatorID animID)
    {
        return animID.GetNumericalID();
    }

    public static implicit operator AnimatorID(string animID)
    {
        return new AnimatorID() {id = animID};
    }

    public override string ToString()
    {
        return string.Format("'{0},{1}'", id, GetNumericalID());
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
