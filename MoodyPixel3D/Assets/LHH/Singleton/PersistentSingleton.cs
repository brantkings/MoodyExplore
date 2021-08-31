using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Singleton
{
    public class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}