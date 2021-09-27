using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Unity
{
    public class Instantiator<T> : MonoBehaviour where T:Object
    {
        public enum HowToGetRotation
        {
            OwnTransform,
            CustomTransform,
            Random,
            Identity
        }

        [Space()]
        public T prefab;

        [Space()]
        public Transform customPositionOrigin;
        public Vector3 globalOffset;
        public Transform customRotationOrigin;
        public HowToGetRotation howToGetRotation;
        public bool thisIsParent;

        protected virtual Vector3 GetPosition()
        {
            if (customPositionOrigin != null) return customPositionOrigin.position + globalOffset;
            else return transform.position + globalOffset;
        }

        protected virtual Quaternion GetRotation()
        {
            switch (howToGetRotation)
            {
                case HowToGetRotation.OwnTransform:
                    return transform.rotation;
                case HowToGetRotation.CustomTransform:
                    return customRotationOrigin.rotation;
                case HowToGetRotation.Random:
                    return Quaternion.Euler(Random.Range(0f,360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                default:
                    return Quaternion.identity;
            }
        }

        protected virtual Transform GetParent()
        {
            return thisIsParent ? transform : null;
        }

        public void JustInstantiate()
        {
            Instantiate();
        }
        
        public T Instantiate()
        {
            return this.Instantiate(prefab, GetPosition(), GetRotation(), GetParent());
        }

        public T Instantiate(Vector3 position, Quaternion rotation)
        {
            return this.Instantiate(prefab, position, rotation, GetParent());
        }

        /// <summary>
        /// This should be overrided to use Object Pooling. Instantiate and return a new or reused intance of T in the desired place.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual T Instantiate(T prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            return GameObject.Instantiate<T>(prefab, position, rotation, parent);
        }
    }
}
