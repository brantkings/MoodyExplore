using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginDestinationFeedback : CreateableSingleton<OriginDestinationFeedback>
{
    public Rigidbody regularFeedbackPrefab;

    [System.Serializable]
    public struct Data
    {
        public float caughtDistance;
        public float forceAttractionBegin;
        public float forceAttractionEnd;
        public float forceAttractionYMultiplier;
        public float durationForce;
        public float minimalTime;
        public Vector3 offsetDestination;

        public static Data DefaultValue
        {
            get
            {
                return new Data()
                {
                    caughtDistance = 0.25f,
                    forceAttractionBegin = 1f,
                    forceAttractionEnd = 10f,
                    forceAttractionYMultiplier = 0.25f,
                    durationForce = 2f,
                    minimalTime = 1f,
                    offsetDestination = Vector3.up,
                };
            }
        }
    }

    public Data defaultValue = Data.DefaultValue;



    public Rigidbody CreateFeedback(Vector3 originPos, Quaternion originRot, Transform destination, Vector3 initialVelocity,  Data data, System.Action<Transform> OnFinish)
    {
        return CreateFeedback(regularFeedbackPrefab, originPos, originRot, destination, initialVelocity, data, OnFinish);
    }

    public Rigidbody CreateFeedback(Rigidbody prefab, Vector3 originPos, Quaternion originRot, Transform destination, Vector3 initialVelocity, Data data, System.Action<Transform> OnFinish)
    {
        Rigidbody instance = Instantiate(prefab, originPos, originRot);
        instance.AddForce(initialVelocity, ForceMode.VelocityChange);
        StartCoroutine(FeedbackRoutine(instance, destination, data, OnFinish));
        return instance;
    }

    public IEnumerator FeedbackRoutine(Rigidbody body, Transform destination, Data data, System.Action<Transform> OnFinish)
    { 
        Debug.LogWarningFormat("Creating particle yes m'am {0} to {1}", body, destination);
        float count = 0;
        Vector3 distance; float caughtDistanceSqrd;
        do
        {
            distance = destination.position + data.offsetDestination - body.position;
            caughtDistanceSqrd = data.caughtDistance * data.caughtDistance;

            float forceAttractionMag = Mathf.Lerp(data.forceAttractionBegin, data.forceAttractionEnd, Mathf.InverseLerp(0f, data.durationForce, count));
            Vector3 forceAttraction = distance.normalized * forceAttractionMag;
            forceAttraction.y *= data.forceAttractionYMultiplier;
            body.AddForce(forceAttraction, ForceMode.Force);
            yield return null;
            count += Time.deltaTime;
        }
        while (count < data.minimalTime || distance.sqrMagnitude > caughtDistanceSqrd);

        Destroy(body.gameObject);
        Debug.LogWarningFormat("Hey adding thought to {0}", destination);
        if(destination != null && OnFinish != null) OnFinish(destination);
    }
}
