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
    public UnityEvent OnDifferentInteractor;

    private bool? _cacheNull = null;
    private Transform _currentTarget;

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

        if(_currentTarget != newFirst)
        {
            DispatchDifferent(newFirst);
        }

        if (newFirst != null)
            StartCoroutine(KeepInPlace(newFirst.GetInteractablePosition()));
        _cacheNull = isNull;
    }

    private IEnumerator KeepInPlace(Transform where)
    {
        if (where == null) yield break;

        _currentTarget = where;
        while(_currentTarget == where && keepInPlace != null)
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
        OnHasInteractor.Invoke(!isNull);
    }

    private void DispatchDifferent(MoodInteractable interactable)
    {
        if(interactable != null)
        {
            OnDifferentInteractor.Invoke();
        }
    }
}
