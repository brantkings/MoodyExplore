using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodEventInteractable : MoodInteractable
{
    public MoodEvent[] whatHappen;

    public ScriptableEvent[] feedback;

    [SerializeField]
    private Transform _feedbackLocationOverride;

    public bool directToInteractable = true;
    public float directToInteractableDuration = 0.25f;

    public Transform FeedbackTransform
    {
        get
        {
            if (_feedbackLocationOverride != null) return _feedbackLocationOverride;
            else return transform;
        }
    }

    
    public override void Interact(MoodInteractor interactor)
    {
        if (directToInteractable) 
            interactor.GetComponentInParent<MoodPawn>()?.RotateDash(Vector3.ProjectOnPlane(FeedbackTransform.position  - interactor.transform.position, Vector3.up), directToInteractableDuration);

        foreach (var evt in whatHappen)
        {
            evt.Execute(FeedbackTransform);
        }

        feedback.Invoke(FeedbackTransform);
    }
    
}
