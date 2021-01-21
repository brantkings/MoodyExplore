using System;
using UnityEngine;

namespace Code.Animation
{
    [System.Serializable]
    public struct AnimatorID
    {
        [SerializeField] private string _id;
        private int _hash;

        public AnimatorID(string id)
        {
            _id = id;
            _hash = 0;
        }

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(_id);
        }

        public int GetHash()
        {
            if (_hash == 0 && IsValid()) _hash = Animator.StringToHash(_id);
            return _hash;
        }
    
        public static implicit operator int(AnimatorID id)
        {
            return id.GetHash();
        }
        
        public static implicit operator AnimatorID(string id)
        {
            return new AnimatorID(id);
        }

    
        /*public static explicit operator string(AnimationID id)
        {
            return id._id;
        }*/

    }
}
