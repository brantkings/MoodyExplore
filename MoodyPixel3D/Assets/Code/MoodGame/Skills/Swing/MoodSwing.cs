using FMOD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(menuName ="Mood/Swing Data", fileName = "Swing_")]
public class MoodSwing : ScriptableObject
{
    public static int CACHE_SIZE = 8;

    [System.Serializable]
    public struct MoodSwingNode
    {
        public Vector3 localPosition;
        public float radius;
        public Vector3 direction;
        public float delay;
    }


    public struct MoodSwingResult
    {
        public Vector3 hitDirection;
        public Vector3 hitPosition;
        public Collider collider;

        public static MoodSwingResult Merge(MoodSwingResult a, MoodSwingResult b)
        {
            return new MoodSwingResult
            {
                hitPosition = Vector3.Lerp(a.hitPosition, b.hitPosition, 0.5f),
                hitDirection = Vector3.Slerp(a.hitDirection, b.hitDirection, 0.5f)
            };
        }

        public static MoodSwingResult DefaultValue
        {
            get
            {
                return new MoodSwingResult();
            }
        }
    }

    public MoodSwingNode[] data;
    public LayerMask layer;

    private static Collider[] _colliderCache = new Collider[CACHE_SIZE];
    private static Dictionary<Collider, MoodSwingResult> _resultsCache = new Dictionary<Collider, MoodSwingResult>(CACHE_SIZE);

    public void SetData(IEnumerable<MoodSwingNode> nodes, int length)
    {
        data = new MoodSwingNode[length];
        int i = 0;
        foreach(MoodSwingNode node in nodes)
        {
            data[i++] = node;
        }
    }

    public static MoodSwingResult? TryHitGetFirst(MoodSwing swing, Vector3 posOrigin, Quaternion rotOrigin)
    {
        float currentDelay = 0f;
        for (int i = 0, len = swing.data.Length;i<len;i++)
        {
            MoodSwingNode node = swing.data[i];
            int result = Physics.OverlapSphereNonAlloc(posOrigin + node.localPosition, node.radius, _colliderCache, swing.layer.value, QueryTriggerInteraction.Collide);
            if (result > 0)
            {
                for(int j = 0,lenA = Mathf.Min(result, CACHE_SIZE);j<lenA;j++)
                {
                    return GetResultFrom(posOrigin, rotOrigin, node, _colliderCache[j]);
                }
            }
            currentDelay += node.delay;
        }
        return null;
    }

    public static IEnumerable<MoodSwingResult> TryHitMerged(MoodSwing swing, Vector3 posOrigin, Quaternion rotOrigin)
    {
        float currentDelay = 0f;
        _resultsCache.Clear();
        for (int i = 0, len = swing.data.Length; i < len; i++)
        {
            MoodSwingNode node = swing.data[i];
            int result = Physics.OverlapSphereNonAlloc(posOrigin + node.localPosition, node.radius, _colliderCache, swing.layer.value, QueryTriggerInteraction.Collide);
            if (result > 0)
            {
                for (int j = 0, lenA = Mathf.Min(result, CACHE_SIZE); j < lenA; j++)
                {
                    Collider collider = _colliderCache[j];
                    MoodSwingResult newResult = GetResultFrom(posOrigin, rotOrigin, node, collider);
                    if (_resultsCache.ContainsKey(collider))
                    {
                        _resultsCache[collider] = MoodSwingResult.Merge(_resultsCache[collider], newResult);
                    }
                    else
                    {
                        _resultsCache.Add(collider, newResult);
                    }
                }
            }
            currentDelay += node.delay;
        }
        foreach (MoodSwingResult r in _resultsCache.Values) yield return r;
    }

    private static MoodSwingResult GetResultFrom(Vector3 posOrigin, Quaternion rotOrigin, MoodSwingNode node, Collider col)
    {
        return new MoodSwingResult()
        {
            hitDirection = rotOrigin * node.direction,
            hitPosition = Vector3.Lerp(posOrigin + node.localPosition, col.transform.position, 0.5f),
            collider = col
        };

    }

    public float GetRange()
    {
        float range = 0f;
        foreach(var d in data)
        {
            range = Mathf.Max(range, d.localPosition.magnitude + d.radius);
        }
        return range;
    }
}
