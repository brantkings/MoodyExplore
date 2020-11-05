using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LHH.Structures
{

    public struct Lock<T>
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
    }
}
