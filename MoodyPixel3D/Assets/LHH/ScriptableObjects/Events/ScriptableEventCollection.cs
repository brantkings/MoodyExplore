using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.ScriptableObjects.Events
{

    [CreateAssetMenu(menuName = "Long Hat House/Events/Generic Event Collection", fileName = "E_")]
    public class ScriptableEventCollection : ScriptableEvent
    {
        ScriptableEvent[] collection;

        public override void Invoke(Transform where)
        {
            collection.Invoke(where);
        }

        public void Invoke(Transform where, Vector3 position, Quaternion rotation)
        {
            collection.Invoke(where, position, rotation);
        }
    }
}
