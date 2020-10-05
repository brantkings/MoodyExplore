using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitUntilObjectDeactivated : CustomYieldInstruction
{
    GameObject _obj;
    bool _deactivatedSelfOnly;

    public WaitUntilObjectDeactivated(GameObject obj, bool onlyDeactivatedSelf = false)
    {
        _obj = obj;
        _deactivatedSelfOnly = onlyDeactivatedSelf;
    }

    public override bool keepWaiting
    {
        get
        {
            if (_obj == null) return false;
            else return IsActive(_obj);
        }
    }

    private bool IsActive(GameObject o)
    {
        if(_deactivatedSelfOnly)
        {
            return o.activeSelf;
        }
        else
        {
            return o.activeInHierarchy;
        }
    }
}
public class OrderedSelfDeactivatedObjects : OrderedActivatedObjects
{
    protected override IEnumerator WaitCondition(GameObject o)
    { 
        yield return new WaitUntilObjectDeactivated(o);
    }
}
