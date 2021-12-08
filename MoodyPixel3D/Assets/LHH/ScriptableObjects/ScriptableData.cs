using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.ScriptableObjects
{

    public interface IData<T>
    {
        public T Data
        {
            get;
        }
    }

    public interface IDataSetter<T> : IData<T>
    {
        new public T Data
        {
            get;
            set;
        }
    }

    public abstract class ScriptableData<T> : ScriptableObject, IDataSetter<T>
    {
        [SerializeField] [HideInInspector]
        private bool _created;

        public ScriptableData()
        {
            if(!_created)
            {
                _data = GetDefaultValue();
            }
            _created = true;
        }

        [SerializeField]
        private T _data;

        public T Data { get => _data; set => _data = value; }

        protected abstract T GetDefaultValue();
    }
}
