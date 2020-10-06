using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Long Hat House/Events/Instantiate Prefab", fileName = "INSTANTIATE_")]
public class InstantiateEvent : ScriptableEvent
{
    public InstantiateUtility instantiate;

    public override void Execute(Transform where)
    {
        instantiate.Instantiate(where);
    }
}
