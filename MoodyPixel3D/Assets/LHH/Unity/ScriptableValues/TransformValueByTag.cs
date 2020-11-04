using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity
{
    [CreateAssetMenu(menuName = "Long Hat House/Values/Transform by Tag", fileName = "Transform_Tag_")]
    public class TransformValueByTag : ScriptableValue<Transform>
    {
        [SerializeField]
        private string tag;
        private Transform _cache;

        public override Transform Get()
        {
            if (_cache == null)
            {
                GameObject go = GameObject.FindWithTag(tag);
                if (go != null) _cache = go.transform;

            }
            return _cache;
        }
    }
}
