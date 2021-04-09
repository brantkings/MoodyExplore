using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoodInteractable : MonoBehaviour
{
    public delegate void DelInteractableEvent();

    public event DelInteractableEvent OnInteractableDestroy;

    private void OnDestroy()
    {
        OnInteractableDestroy?.Invoke();
    }

    public abstract void Interact(MoodInteractor interactor);
}
