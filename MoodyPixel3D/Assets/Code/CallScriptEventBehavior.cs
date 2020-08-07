using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CallScriptEventBehaviour : StateMachineBehaviour {

    List<IAnimatorEvent> events;

    private bool _captured;

    public AnimatorID id;

    public void CaptureOutsideScripts(GameObject obj)
    {
        IAnimatorEvent[] allEvents = obj.GetComponentsInChildren<IAnimatorEvent>();
        if(events == null) events = new List<IAnimatorEvent>(allEvents.Length);
        foreach(IAnimatorEvent evt in allEvents)
        {
            if (evt == null || evt.Equals(null)) continue;

            if (id.GetNumericalID() == evt.Id) events.Add(evt);
        }
        _captured = true;
    }

    private bool HasCaptured
    {
        get
        {
            return _captured;
        }
    }

    public void CallScript(Animator anim)
    {
        if (!HasCaptured) CaptureOutsideScripts(anim.gameObject);

        if(events.Count > 0)
        {
            foreach (IAnimatorEvent evt in events) evt.EventFromAnimator();
        }
    }
}
