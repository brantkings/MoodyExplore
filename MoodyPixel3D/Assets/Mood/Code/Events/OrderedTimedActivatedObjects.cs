using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderedTimedActivatedObjects : OrderedActivatedObjects
{
    public float timeAfterEachActivation;

    protected override IEnumerator WaitCondition(GameObject o)
    {
        yield return new WaitForSeconds(timeAfterEachActivation);
    }
}
