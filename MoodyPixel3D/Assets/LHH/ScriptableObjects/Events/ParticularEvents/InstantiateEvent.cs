using LHH.ScriptableObjects.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LHH.ScriptableObjects.Events.ParticularEvents
{
    [CreateAssetMenu(menuName = "Long Hat House/Events/Instantiate Legacy", fileName = "E_Instantiate_", order = 0)]
    public class InstantiateEvent : ScriptableEventPositional<GameObject>
    {
        public GameObject prefab;
        public bool whoInstantiatedIsParent;
        public enum RotationStyle
        {
            Identity,
            AsWhereInstantiated,
            Random,
        }
        public RotationStyle style = RotationStyle.AsWhereInstantiated;
        public Vector3 offsetPosition = Vector3.zero;

        public override GameObject InvokeReturn(Transform where)
        {
            return Instantiate(prefab, where.position + offsetPosition, where.rotation, whoInstantiatedIsParent ? where : null);
        }

        public override GameObject InvokeReturn(Vector3 position, Quaternion rotation)
        {
            return Instantiate(prefab, position + offsetPosition, rotation, null);
        }

        private Quaternion GetRotation(Transform where, RotationStyle how)
        {
            switch (how)
            {
                case RotationStyle.AsWhereInstantiated:
                    return where.rotation;
                case RotationStyle.Random:
                    return Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                default:
                    return Quaternion.identity;
            }
        }
    }
}

