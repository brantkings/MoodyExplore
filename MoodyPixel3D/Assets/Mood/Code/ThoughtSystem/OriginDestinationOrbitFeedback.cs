using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OriginDestinationOrbitFeedback : CreateableSingleton<OriginDestinationOrbitFeedback>
{
    public Rigidbody regularFeedbackPrefab;

    [System.Serializable]
    public struct Data
    {
        public float caughtDistance;
        public float orbitRadius;
        public float orbitY;
        public float durationToOrbit;
        public Ease toOrbitEase;
        public float durationOrbit;
        public float readyToVelocityDuration;
        public float velocityToDestination;
        public Vector3 offsetDestination;

        public static Data DefaultValue
        {
            get
            {
                return new Data()
                {
                    caughtDistance = 0.25f,
                    orbitRadius = 2f,
                    durationOrbit = 2f,
                    durationToOrbit = 1f,
                    velocityToDestination = 10f,
                    readyToVelocityDuration = 0.1f,
                    orbitY = 8f,
                    toOrbitEase = Ease.OutCirc,
                    offsetDestination = Vector3.up * 1.5f,
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
        float timeBegin = Time.time;
        Vector3 distance; float caughtDistanceSqrd;
        Vector3 posOrigin = body.position;
        float t = 0f;

        //Go to orbit
        DOTween.To(() => t, (x) =>
        {
            t = x;
            Vector3 destPos = GetDestinationInOrbit(destination, data.offsetDestination, data.orbitY, data.orbitRadius * t, timeBegin);
            body.position = Vector3.Lerp(posOrigin, destPos, t);
        }, 1f, data.durationToOrbit).SetEase(data.toOrbitEase);
        yield return new WaitForSeconds(data.durationToOrbit);


        //Remain in orbit
        float count = data.durationOrbit;
        while (count > 0f)
        {
            body.position = GetDestinationInOrbit(destination, data.offsetDestination, data.orbitY, data.orbitRadius, timeBegin);
            count -= Time.deltaTime;
            yield return null;
        }


        //Go to destination
        body.velocity = Vector3.zero;
        Vector3 orbitOrigin = body.position;
        do
        {
            distance = destination.position + data.offsetDestination - body.position;
            caughtDistanceSqrd = data.caughtDistance * data.caughtDistance;

            //float forceAttractionMag = Mathf.Lerp(data.forceAttractionBegin, data.forceAttractionEnd, Mathf.InverseLerp(0f, data.durationForce, count));
            float forceAttractionMag = data.velocityToDestination;
            Vector3 forceAttraction = distance.normalized * forceAttractionMag;
            body.velocity = forceAttraction;
            //body.AddForce(forceAttraction, ForceMode.Force);
            yield return null;
        }
        while (distance.sqrMagnitude > caughtDistanceSqrd);

        Destroy(body.gameObject);
        if(destination != null && OnFinish != null) OnFinish(destination);
    }

    private Vector3 GetLocalOrbitPosition(float time0, float angleVelocity = 180f, float angleOffset = 0f)
    {
        float t = Time.time - time0;
        float angle = Mathf.Deg2Rad * (t * angleVelocity + angleOffset);
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private Vector3 GetDestinationInOrbit(Transform destination, Vector3 offsetPosition, float orbitY, float orbitRadius, float time0, float angleVelocity = 180f, float angleOffset = 0f)
    {
        return destination.position + offsetPosition + Vector3.up * orbitY + GetLocalOrbitPosition(time0, angleVelocity, angleOffset) * orbitRadius;
    }
}
