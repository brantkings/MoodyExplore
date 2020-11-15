using FMOD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Mood.Swing.Maker;
using System;

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

    public struct MoodSwingTrailNode
    {
        public Vector3 localPosTop;
        public Vector3 localPosBot;
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
                hitDirection = Vector3.Slerp(a.hitDirection, b.hitDirection, 0.5f),
                collider = a.collider == b.collider ? a.collider : null
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


    //public MoodSwingNode[] data;
    public MoodSwingMaker maker;

    private static Collider[] _colliderCache = new Collider[CACHE_SIZE];
    private static Dictionary<Collider, MoodSwingResult> _resultsCache = new Dictionary<Collider, MoodSwingResult>(CACHE_SIZE);

    /*public void SetData(IEnumerable<MoodSwingNode> nodes, int length)
    {
        data = new MoodSwingNode[length];
        int i = 0;
        foreach(MoodSwingNode node in nodes)
        {
            data[i++] = node;
        }
    }*/


    public IEnumerable<MoodSwingTrailNode> GetTrail()
    {
        return maker.Trail;
    }

    public int GetTrailLength()
    {
        return maker.TrailLength;
    }

    public MoodSwingResult? TryHitGetBest(Vector3 posOrigin, Quaternion rotOrigin, LayerMask layer, Vector3 desiredDirection)
    {
        float minAngle = float.MaxValue;
        MoodSwingResult? result = null;
        foreach(MoodSwingResult r in TryHitMerged(posOrigin, rotOrigin, layer))
        {
            Vector3 direction = r.collider.ClosestPoint(posOrigin) - posOrigin;
            float angle = Vector3.Angle(direction, desiredDirection);
            if(angle < minAngle)
            {
                result = r;
                minAngle = angle;
            }            
        }
        return result;
    }

    public MoodSwingResult? TryHitGetFirst(Vector3 posOrigin, Quaternion rotOrigin, LayerMask layer)
    {
        float currentDelay = 0f;
        foreach(MoodSwingNode node in maker.Nodes)
        { 
            LHH.Utils.DebugUtils.DrawCircle(posOrigin + node.localPosition, node.radius, rotOrigin * Vector3.up, Color.black, 1f);
            int result = Physics.OverlapSphereNonAlloc(posOrigin + node.localPosition, node.radius, _colliderCache, layer.value, QueryTriggerInteraction.Collide);
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

    public IEnumerable<MoodSwingResult> TryHitMerged(Vector3 posOrigin, Quaternion rotOrigin, LayerMask layer)
    {
        float currentDelay = 0f;
        _resultsCache.Clear();
        foreach(MoodSwingNode node in maker.Nodes)
        { 
            LHH.Utils.DebugUtils.DrawCircle(posOrigin + rotOrigin * node.localPosition, node.radius, rotOrigin * Vector3.up, Color.black, 1f);
            int result = Physics.OverlapSphereNonAlloc(posOrigin + rotOrigin * node.localPosition, node.radius, _colliderCache, layer.value, QueryTriggerInteraction.Collide);
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
        UnityEngine.Debug.LogFormat("Found {0} results woo", _resultsCache.Values.Count);
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
        foreach(var d in maker.Nodes)
        {
            range = Mathf.Max(range, Vector3.ProjectOnPlane(d.localPosition, Vector3.up).magnitude + d.radius);
        }
        return range;
    }
}
