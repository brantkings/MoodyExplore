using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using LHH.ScriptableObjects.Events;

[Serializable]
public struct FlyingThoughtInstance
{
    public FlyingThought flyingThought;
    public FlyingThought.FlyingThoughtData data;

    public bool CanDo()
    {
        return flyingThought != null;
    }

    public void Do(Transform origin)
    {
        flyingThought.InvokeReturnExtraParameter(origin, data);
    }

    public void InstatiateFlyingThought(Transform origin, Transform destination)
    {
        flyingThought.InvokeReturnExtraParameter(origin, data.GetCopy().SetDestination(destination));
    }
}

[CreateAssetMenu(menuName = "Mood/Thought System/Flying Thought", fileName = "E_FlThought_", order = 0)]
public class FlyingThought : ScriptableEvent<Rigidbody, FlyingThought.FlyingThoughtData>
{
    [System.Serializable]
    public struct FlyingThoughtData
    {
        public Transform destination;
        public Thought thought;
        public ThoughtSystemController.ThoughtPlacement where;

        public FlyingThoughtData GetCopy()
        {
            return new FlyingThoughtData()
            {
                destination = destination,
                thought = thought,
                where = where,
            };

        }

        public FlyingThoughtData SetDestination(Transform d)
        {
            destination = d;
            return this;
        }
    }

    [Space]
    public Rigidbody prefab;
    public float initialVelocity = 5f;

    public OriginDestinationFeedback.Data feedbackData = OriginDestinationFeedback.Data.DefaultValue;
    public OutlineMaterialFeedback.Data outlineFeedbackData;

    public override FlyingThoughtData GetDefaultExtraParameter()
    {
        return new FlyingThoughtData()
        {
            destination = MoodPlayerController.Instance.Pawn.ObjectTransform,
            thought = null,
        };
    }

    public override Rigidbody InvokeReturnExtraParameter(Transform origin, FlyingThoughtData data)
    {
        Vector3 distance = origin.position - data.destination.position;
        Thought thoughtToAdd = data.thought;
        Debug.LogFormat("Prefab is {0}, origin is {1}, data dest is {2}, thought is {3}", prefab, origin, data.destination, thoughtToAdd);
        Rigidbody newRB = OriginDestinationFeedback.Instance.CreateFeedback(prefab, origin.position, origin.rotation, data.destination, (Quaternion.Euler(45f, 45f, 0f) * (distance + Vector3.up)).normalized *  initialVelocity, feedbackData,
            (Transform destination)=>
            {
                MoodPawn pawn = destination.GetComponentInParent<MoodPawn>();
                ThoughtSystemController thoughtSystem = pawn?.GetComponentInChildren<ThoughtSystemController>();
                Debug.LogFormat("Adding {0} to {1}", thoughtToAdd, thoughtSystem);
                if (thoughtSystem != null)
                {
                    thoughtSystem.AddThought(thoughtToAdd, pawn, outlineFeedbackData);
                }
            });
        return newRB; 
    }

    public static FlyingThoughtData MakeData(Thought thought, Transform target)
    {
        return new FlyingThoughtData()
        {
            thought = thought,
            destination = target
        };

    }
}
