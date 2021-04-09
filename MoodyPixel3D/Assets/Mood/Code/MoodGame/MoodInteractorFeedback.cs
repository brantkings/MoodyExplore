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
   
   protected override void OnChange(MoodInteractable newfirst)
   {
       base.OnChange(newfirst);
       
       bool isNull = (newfirst == null);
       if (_hasCache)
       {
           if(isNull != _cacheNull) DispatchNull(!isNull);
       }
       else
       {
           DispatchNull(!isNull);
       }
       _hasCache = true;
       _cacheNull = isNull;

        //Events
        if(_before != newfirst)
        {
            if(_before != null)
            {
                _before.OnInteractableDestroy -= OnInteractableDestroy;
            }
            if (!isNull)
            {
                newfirst.OnInteractableDestroy += OnInteractableDestroy;
            }
        }


        _before = newfirst;
    }

    private void OnInteractableDestroy()
    {
        DispatchNull(false);
    }

    private void DispatchNull(bool isNull)
   {
       Debug.LogFormat("{0} is null? {1}", this, isNull);
       OnHasInteractor.Invoke(isNull);
   }
}
