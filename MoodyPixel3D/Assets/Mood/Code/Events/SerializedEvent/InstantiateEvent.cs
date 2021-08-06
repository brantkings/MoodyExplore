using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.ScriptableObjects.Events;

[CreateAssetMenu(menuName = "Long Hat House/Events/Instantiate Prefab", fileName = "INSTANTIATE_")]
public class InstantiateEvent : ScriptableEvent
{
    public InstantiateUtility instantiate;

    public override void Invoke(Transform where)
    {
        instantiate.Instantiate(where);
    }
}
