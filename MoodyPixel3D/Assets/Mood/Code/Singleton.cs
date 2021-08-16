using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

#if UNITY_EDITOR
    public bool debugCantFindSingleton = true;
    private static bool alreadyToldCantFind = false;

#endif

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
                    if(!alreadyToldCantFind)
                    {
                        Debug.LogErrorFormat("Couldnt find singleton of type {0}!", typeof(T));
                        alreadyToldCantFind = true;
                    }
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
