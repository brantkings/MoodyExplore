using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class OrderedActivatedObjects : MonoBehaviour
{

    public IEnumerable<GameObject> GetOrderedObjects()
    {
        foreach(Transform child in transform)
        {
            yield return child.gameObject;
        }
    }

    private void Awake()
    {
        DeactivateAll();
    }

    private void DeactivateAll()
    {
        foreach (GameObject o in GetOrderedObjects()) o.SetActive(false);
    }

    public void BeginActivation()
    {
        if(isActiveAndEnabled)
            StartCoroutine(ActivateRoutine());
    }

    public void InterruptActivation()
    {
        StopAllCoroutines();
        DeactivateAll();
    }

    protected abstract IEnumerator WaitCondition(GameObject o);

    private IEnumerator ActivateRoutine()
    {
        yield return null;
        foreach(GameObject o in GetOrderedObjects())
        {
            o.SetActive(true);
            yield return WaitCondition(o);
        }
    }
}
