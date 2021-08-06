using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.ScriptableObjects.Events
{

    public abstract class ScriptableEvent : ScriptableObject
    {
        public abstract void Invoke(Transform where);

#if UNITY_EDITOR
        public string tagToTest = "MainCamera";
        [ContextMenu("Test Event")]
        public void Test()
        {
            GameObject obj = GameObject.FindWithTag(tagToTest);
            if (obj != null)
            {
                Invoke(GameObject.FindWithTag(tagToTest).transform);
            }
            else
            {
                Debug.LogErrorFormat("Error: Can't find object with tag {0}.", tagToTest);
            }
        }

#endif
    }

    public abstract class ScriptableEventPositional : ScriptableEvent
    {
        public abstract void Invoke(Vector3 position, Quaternion rotation);

        public override void Invoke(Transform where)
        {
            Invoke(where.position, where.rotation);
        }
    }


    public abstract class ScriptableEvent<T> : ScriptableEvent
    {
        public override void Invoke(Transform where)
        {
            InvokeReturn(where);
        }

        public abstract T InvokeReturn(Transform where);
    }

    public abstract class ScriptableEvent<T, X> : ScriptableEvent<T>
    {
        public bool warningOnDefaultParameter = true;

        public abstract X GetDefaultExtraParameter();

        public override T InvokeReturn(Transform where)
        {
            if (warningOnDefaultParameter) Debug.LogWarningFormat(where, "Using default parameter for '{0}' at '{1}'.", this, where);
            return InvokeReturnExtraParameter(where, GetDefaultExtraParameter());
        }

        public abstract T InvokeReturnExtraParameter(Transform where, X extraParameter);
    }

    public abstract class ScriptableEventPositional<T> : ScriptableEventPositional
    {
        public override void Invoke(Vector3 position, Quaternion rotation)
        {
            InvokeReturn(position, rotation);
        }

        public virtual T InvokeReturn(Transform where)
        {
            return InvokeReturn(where.position, where.rotation);
        }

        public abstract T InvokeReturn(Vector3 position, Quaternion rotation);
    }

    public static class ScriptableEventExtensions
    {
        public static void Invoke(this ScriptableEvent[] collection, Transform where)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Length; i < len; i++)
                {
                    ScriptableEvent evt = collection[i];
                    if (evt != null) evt.Invoke(where);
                }
            }
        }

        public static void Invoke(this List<ScriptableEvent> collection, Transform where)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Count; i < len; i++)
                {
                    ScriptableEvent evt = collection[i];
                    if (evt != null) evt.Invoke(where);
                }
            }
        }


        public static void Invoke(this ScriptableEventPositional[] collection, Vector3 position, Quaternion rotation)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Length; i < len; i++)
                {
                    ScriptableEventPositional evt = collection[i];
                    if (evt != null) evt.Invoke(position, rotation);
                }
            }
        }

        public static void Invoke(this List<ScriptableEventPositional> collection, Vector3 position, Quaternion rotation)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Count; i < len; i++)
                {
                    ScriptableEventPositional evt = collection[i];
                    if (evt != null) evt.Invoke(position, rotation);
                }
            }
        }


        public static void Invoke(this ScriptableEvent[] collection, Transform where, Vector3 positionOverride, Quaternion rotationOverride)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Length; i < len; i++)
                {
                    ScriptableEventPositional evtPos = collection[i] as ScriptableEventPositional;
                    if (evtPos != null)
                    {
                        evtPos.Invoke(positionOverride, rotationOverride);
                        continue;
                    }
                    ScriptableEvent evt = collection[i];
                    if (evt != null) evt.Invoke(where);
                }

            }
        }

        public static void Invoke(this List<ScriptableEvent> collection, Transform where, Vector3 positionOverride, Quaternion rotationOverride)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Count; i < len; i++)
                {
                    ScriptableEventPositional evtPos = collection[i] as ScriptableEventPositional;
                    if (evtPos != null)
                    {
                        evtPos.Invoke(positionOverride, rotationOverride);
                        continue;
                    }
                    ScriptableEvent evt = collection[i];
                    if (evt != null) evt.Invoke(where);
                }
            }
        }

        public static void Invoke<T, X>(this List<ScriptableEvent<T, X>> collection, Transform where, X parameter)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Count; i < len; i++)
                {
                    ScriptableEvent<T, X> evt = collection[i];
                    evt.InvokeReturnExtraParameter(where, parameter);
                }
            }
        }
        public static void Invoke<T, X>(this ScriptableEvent<T, X>[] collection, Transform where, X parameter)
        {
            if (collection != null)
            {
                for (int i = 0, len = collection.Length; i < len; i++)
                {
                    ScriptableEvent<T, X> evt = collection[i];
                    evt.InvokeReturnExtraParameter(where, parameter);
                }
            }
        }
    }
}
