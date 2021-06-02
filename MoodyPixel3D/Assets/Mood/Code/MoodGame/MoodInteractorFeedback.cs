using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoodInteractorFeedback : LHH.Structures.InterfaceCaptureFeedback<MoodInteractable>
{
    [System.Serializable]
    public class UnityEventBool : UnityEvent<bool>
    {
      
    }

    public UnityEventBool OnHasInteractor;

    private bool? _cacheNull = null;
    private Transform currentTarget;

    public Transform keepInPlace;

    private MoodInteractable _before;
   
    protected override void OnChange(MoodInteractable interactable)
    {
        base.OnChange(interactable);

        Capture(interactable, out bool isNull);

        //Events
        if(_before != interactable)
        {
            if(_before != null)
            {
                _before.OnInteractableDestroy -= OnInteractableDestroy;
            }
            if (!isNull)
            {
                interactable.OnInteractableDestroy += OnInteractableDestroy;
            }
        }


        _before = interactable;
    }

    private void Capture(MoodInteractable newFirst, out bool isNull)
    {
        isNull = (newFirst == null);
        if (_cacheNull.HasValue)
        {
            if (isNull != _cacheNull) DispatchNull(newFirst, isNull);
        }
        else
        {
            DispatchNull(newFirst, isNull);
        }
        if (newFirst != null)
            StartCoroutine(KeepInPlace(newFirst.GetInteractablePosition()));
        _cacheNull = isNull;
    }

    private IEnumerator KeepInPlace(Transform where)
    {
        if (where == null) yield break;

        currentTarget = where;
        while(currentTarget == where && keepInPlace != null)
        {
            if (where != null)
                keepInPlace.transform.position = where.position;
            else keepInPlace.transform.localPosition = Vector3.zero;
            yield return null;
        }
    }

    private void OnInteractableDestroy()
    {
        Capture(null, out bool isNull);
    }

    private void DispatchNull(MoodInteractable interactable, bool isNull)
    {
        Debug.LogWarningFormat("{0} is dispatching null {1} to {2}", this, isNull, interactable);
        OnHasInteractor.Invoke(!isNull);
    }
}
