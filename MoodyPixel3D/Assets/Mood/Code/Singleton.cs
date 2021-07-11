using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<T>();
#if UNITY_EDITOR
                if(_instance == null)
                {
                    Debug.LogErrorFormat("Couldnt find singleton of type {0}!", typeof(T));
                }
#endif
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null) _instance = this as T;
        else if (_instance != this) Destroy(this);
    }




}
