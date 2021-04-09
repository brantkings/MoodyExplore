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

   private bool _cacheNull;
   private bool _hasCache;

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
        if (_hasCache)
        {
            if (isNull != _cacheNull) DispatchNull(!isNull);
        }
        else
        {
            DispatchNull(!isNull);
        }
        _hasCache = true;
        _cacheNull = isNull;
    }

    private void OnInteractableDestroy()
    {
        Capture(null, out bool isNull);
    }

    private void DispatchNull(bool isNull)
   {
       Debug.LogFormat("{0} is null? {1}", this, isNull);
       OnHasInteractor.Invoke(isNull);
   }
}
