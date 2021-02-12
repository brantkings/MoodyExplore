using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LHH.Structures
{

    public class Lock<T>
    {
        private HashSet<T> _locks;

        public delegate void DelLockEvent();
        public event DelLockEvent OnLock;
        public event DelLockEvent OnUnlock;

        private static HashSet<T> InitHashSet()
        {
            return new HashSet<T>();
        }


        public bool IsLocked()
        {
            return _locks != null && _locks.Count > 0;
        }

        public bool Add(T id)
        {
            bool wasEmpty;
            if (_locks == null)
            {
                _locks = InitHashSet();
                wasEmpty = true;
            }
            else
            {
                wasEmpty = _locks.Count <= 0;
            }
            bool changed = _locks.Add(id);
            if (wasEmpty && changed) OnLock?.Invoke();
            return changed;
        }

        public bool Remove(T id)
        {
            if (_locks == null) return false;
            bool changed = _locks.Remove(id);
            bool isEmpty = _locks.Count <= 0;
            if (changed && isEmpty) OnUnlock?.Invoke();
            return changed;
        }

        public bool RemoveAll()
        {
            if (_locks == null || _locks.Count <= 0) return false;
            _locks.Clear();
            OnUnlock?.Invoke();
            return true;
        }

        public override string ToString()
        {
            string lockStr = "";
            bool did = false;
            if(_locks != null)
            {
                foreach(T l in _locks)
                {
                    if (did) lockStr += ", ";
                    lockStr += l.ToString();
                    did = true;
                }
            }
            return $"{(IsLocked() ? "Locked" : "Unlocked")} - [{lockStr}]";
        }
    }
}
