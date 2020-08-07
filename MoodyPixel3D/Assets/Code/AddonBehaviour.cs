using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AddonBehaviour<T> : MonoBehaviour
{
    protected T Addon
    {
        private set; get;
    }

    protected virtual void Awake()
    {
        Addon = GetComponent<T>();
    }
}

public abstract class AddonBehaviour<T, K> : MonoBehaviour
{
    protected T FirstAddon
    {
        private set; get;
    }

    protected K SecondAddon
    {
        private set; get;
    }

    protected virtual void Awake()
    {
        FirstAddon = GetComponent<T>();
        SecondAddon = GetComponent<K>();
    }
}
