using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }
                return _instance;
            }
        }

        protected virtual T FindObject()
        {
            return FindObjectOfType<T>();
        }

        protected virtual void Awake()
        {
            if (_instance == null) _instance = this as T;
            else if (_instance != this) Destroy(this);
        }
    }
}