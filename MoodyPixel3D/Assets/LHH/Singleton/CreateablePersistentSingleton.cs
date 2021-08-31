using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Singleton
{
    public class CreateablePersistentSingleton<T> : MonoBehaviour where T: MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObject();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Find the object of the type of the singleton, only called if there's no object.
        /// </summary>
        /// <returns></returns>
        protected static T FindObject()
        {
            GameObject newObj = new GameObject($"[{typeof(T)}] CreatedSingleton");
            return newObj.AddComponent<T>();
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(this);
            }
            else if (_instance != this) Destroy(this);
        }
    }
}