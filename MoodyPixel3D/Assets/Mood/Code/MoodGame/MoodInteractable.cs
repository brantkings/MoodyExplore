using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodInteractable : MonoBehaviour
{
    public delegate void DelInteractableEvent();

    public event DelInteractableEvent OnInteractableDestroy;

    public Transform interactablePosition;

    private void OnDestroy()
    {
        OnInteractableDestroy?.Invoke();
    }

    public virtual Transform GetInteractablePosition()
    {
        return interactablePosition != null ? interactablePosition : transform;
    }

    public abstract void Interact(MoodInteractor interactor);

    public virtual bool CanBeInteracted(MoodInteractor t)
    {
        return !IsBeingInteracted();
    }

    public abstract bool IsBeingInteracted();
}
