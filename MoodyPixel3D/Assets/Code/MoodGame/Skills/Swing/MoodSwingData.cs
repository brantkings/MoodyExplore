using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoodSwing : ScriptableObject
{
    public struct MoodSwingNode
    {
        public Vector3 localPosition;
        public float radius;
        public Vector3 direction;
        public float delay;
    }

    public struct MoodSwingData
    {
        public MoodSwingNode[] data;
        public LayerMask layer;
    }

    public struct MoodSwingResult
    {
        public Vector3 hitDirection;
        public Vector3 hitPosition;
    }

    public MoodSwingData data;

    public static Collider[] _colliderCache = new Collider[8];

    public static MoodSwingResult TryHit(MoodSwingData swing, Vector3 posOrigin, Quaternion rotOrigin)
    {

        float currentDelay = 0f;
        for (int i = 0, len = swing.data.Length;i<len;i++)
        {
            MoodSwingNode data = swing.data[i];
            int result = Physics.OverlapSphereNonAlloc(posOrigin + data.localPosition, data.radius, _colliderCache, swing.layer.value, QueryTriggerInteraction.Collide);
            if (result > 0)
            {
                for(int j = 0;j<result;j++)
                {
                }
            }
        }

        return new MoodSwingResult() { };
    }
}
