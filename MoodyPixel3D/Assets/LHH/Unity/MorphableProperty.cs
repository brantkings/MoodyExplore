using UnityEngine;

namespace LHH.Unity
{
    public interface IPropertyValueGetter<T>
    {
        T Get();
    }

    public abstract class ScriptableValue<T> : ScriptableObject, IPropertyValueGetter<T>
    {
        public abstract T Get();

        public static implicit operator T(ScriptableValue<T> v)
        {
            return v.Get();
        }
    }

    public class ScriptableDirectValue<T> : ScriptableValue<T>
    {
        [SerializeField]
        private T _value;

        public override T Get()
        {
            return _value;
        }
    }  

    public abstract class MorphablePropertyBase
    {
        public abstract Object GetOverrider();

        /// <summary>
        /// Set the overrider object and return if it changed or not.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract bool SetOverrider(Object obj);

        public abstract bool HasOverrider();
    }

    [System.Serializable]
    public class MorphableProperty<T> : MorphablePropertyBase, IPropertyValueGetter<T>
    {
        [SerializeField]
        private T _directProperty;
        [SerializeField]
        private ScriptableValue<T> _getter;

        public T Get()
        {
            return _getter != null && !_getter.Equals(null) ? _getter.Get() : _directProperty;
        }

        public override Object GetOverrider()
        {
            return _getter as Object;
        }

        public override bool SetOverrider(Object obj)
        {
            ScriptableValue<T> newPropGetter = obj as ScriptableValue<T>;
            bool changed = newPropGetter != _getter;
            _getter = newPropGetter;
            return changed;
        }

        public override bool HasOverrider()
        {
            return _getter != null;
        }

        public static implicit operator T(MorphableProperty<T> prop)
        {
            return prop.Get();
        }

        public static implicit operator MorphableProperty<T>(T prop)
        {
            return new MorphableProperty<T>()
            {
                _directProperty = prop,
                _getter = null
            };
        }
    }
}

