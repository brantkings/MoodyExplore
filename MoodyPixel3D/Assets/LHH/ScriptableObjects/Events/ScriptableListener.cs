using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.ScriptableObjects.Events
{

    [CreateAssetMenu(menuName = "Long Hat House/Listener/Basic Listener", fileName = "L_", order = 0)]
    public class ScriptableListener : ScriptableEvent
    {
        public delegate void DelExecute();

        private event DelExecute TheEvent;

        public bool debug;

        public ScriptableEvent[] componentlessEvents;

        public void Subscribe(DelExecute func)
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just subscribed!", this);
            TheEvent += func;
        }

        public void Unsubscribe(DelExecute func)
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just unsubscribed!", this);
            TheEvent -= func;
        }

        public void Clear()
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just cleared!", this);
            TheEvent = null;
        }

        public override void Invoke(Transform where)
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just executed by {1}!", this, where);
            TheEvent?.Invoke();
            componentlessEvents.Invoke(where);
        }

    }

    public class ScriptableListener<T> : ScriptableObject
    {
        public delegate void DelExecute(T param);

        private event DelExecute TheEvent;

        public bool debug;

        public void Subscribe(DelExecute func)
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just subscribed!", this);
            TheEvent += func;
        }

        public void Unsubscribe(DelExecute func)
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just unsubscribed!", this);
            TheEvent -= func;
        }

        public void Clear()
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just cleared!", this);
            TheEvent = null;
        }

        public void Execute(T param)
        {
            if (debug) Debug.LogFormat("[LISTENER] {0} just executed with {1}!", this, param);
            TheEvent?.Invoke(param);
        }

    }
}
