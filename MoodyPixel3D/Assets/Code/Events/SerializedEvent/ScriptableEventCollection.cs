using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Long Hat House/Events/Generic Event Collection", fileName = "E_")]
public class SerializedEventCollection : ScriptableEvent
{
    ScriptableEvent[] collection;

    public override void Execute(Transform where)
    {
        collection.Execute(where);
    }
}
