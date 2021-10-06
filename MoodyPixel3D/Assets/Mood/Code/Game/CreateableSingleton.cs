using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Make a Inherit from this class using your own type as T.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CreateableSingleton<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<T>();

                if(_instance == null)
                {
                    string name = "M";
    #if UNITY_EDITOR
                    name = string.Format("[{0}]", typeof(T).ToString());
    #endif
                    GameObject obj = new GameObject(name);
                    _instance = obj.AddComponent<T>();
                }

            }
            return _instance;
        }
    }
}
