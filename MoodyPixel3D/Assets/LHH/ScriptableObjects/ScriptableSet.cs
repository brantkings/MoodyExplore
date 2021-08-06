using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Unity;
using System.Linq;

namespace LHH.ScriptableObjects
{
    public abstract class ScriptableSet<T> : ScriptableValue<T>, IEnumerable<T>
    {
        public enum GetMeaning
        {
            First,
            Random,
            Last
        }
        public GetMeaning howToGet = GetMeaning.Random;

        public override T Get()
        {
            switch (howToGet)
            {
                case GetMeaning.First:
                    return this.FirstOrDefault();
                case GetMeaning.Random:
                    return GetRandom();
                case GetMeaning.Last:
                    return this.LastOrDefault();
                default:
                    return GetRandom();
            }
        }

        public abstract T GetRandom();
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int Length
        {
            get;
        }

        public int Count
        {
            get
            {
                return Length;
            }
        }
    }

    public class ScriptableArraySet<T> : ScriptableSet<T>
    {
        [SerializeField]
        protected T[] set;

        public override T GetRandom()
        {
            int randomIndex = Mathf.FloorToInt(Random.Range(0f, set.Length));
            return set[randomIndex];
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)set).GetEnumerator();
        }

        public override int Length => set.Length;
    }

    public class ScriptableSetWeighted<T> : ScriptableSet<T>
    {
        [System.Serializable]
        public class WeightedElement
        {
            public int weight = 1;
            public T element;
        }


        [SerializeField]
        protected WeightedElement[] set;

        public override int Length => set.Length;

        public override T GetRandom()
        {
            int sumOfWeights = 0;
            foreach (var w in set) sumOfWeights += w.weight;
            int randomIndex = Mathf.FloorToInt(Random.Range(0f, sumOfWeights));
            foreach (var w in set) 
            {
                randomIndex -= w.weight; 
                if (randomIndex <= 0) return w.element;
            }
            return default(T);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            for (int i = 0, len = set.Length; i < len; i++) yield return set[i].element;
        }
    }
}
