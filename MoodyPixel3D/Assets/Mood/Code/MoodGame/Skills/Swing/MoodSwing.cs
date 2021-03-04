using FMOD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            //UnityEngine.Debug.LogFormat("Merge {0} and {1} result is {2}", a.hitPosition, b.hitPosition, Vector3.Lerp(a.hitPosition, b.hitPosition, 0.5f));
            return new MoodSwingResult
            {
                hitPosition = Vector3.Lerp(a.hitPosition, b.hitPosition, 0.5f),
                hitDirection = Vector3.Slerp(a.hitDirection, b.hitDirection, 0.5f),
                collider = a.collider == b.collider ? a.collider : null
            };
        }

        public bool IsValid()
        {
            return collider != null;
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
        int b=0;
        foreach(MoodSwingNode node in maker.Nodes)
        { 
            LHH.Utils.DebugUtils.DrawCircle(GetCorrectPosition(posOrigin, rotOrigin, node), node.radius, rotOrigin * Vector3.up, Color.black, 1f);
            int result = Physics.OverlapSphereNonAlloc(posOrigin + rotOrigin * node.localPosition, node.radius, _colliderCache, layer.value, QueryTriggerInteraction.Collide);
            if (result > 0)
            {
                for (int j = 0, lenA = Mathf.Min(result, CACHE_SIZE); j < lenA; j++)
                {
                    Collider collider = _colliderCache[j];
                    MoodSwingResult newResult = GetResultFrom(posOrigin, rotOrigin, node, collider);
                    //UnityEngine.Debug.LogFormat("MoodSwing {0}: {1} (from {2}) collider is found. Has collider? {3} ({4} iteration)", this.name, collider.name, 
                        //collider.GetComponentInParent<MoodPawn>()?.name, _resultsCache.ContainsKey(collider), ++b);
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
        _resultsCache.Clear();
    }

    private static Vector3 GetCorrectPosition(Vector3 posOrigin, Quaternion rotOrigin, MoodSwingNode node)
    {
        return posOrigin + rotOrigin * node.localPosition;
    }


    private static MoodSwingResult GetResultFrom(Vector3 posOrigin, Quaternion rotOrigin, MoodSwingNode node, Collider col)
    {
        return new MoodSwingResult()
        {
            hitDirection = rotOrigin * node.direction,
            hitPosition = Vector3.Lerp(GetCorrectPosition(posOrigin, rotOrigin, node), col.transform.position, 0.5f),
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

    public delegate void DelUpdateVectors(ref Vector3 top, ref Vector3 bot, int index, int length);

    public static void MakeVertexTrailRightToLeft(MoodSwing data, List<Vector3> vertexData, List<int> triangleData, DelUpdateVectors changeFunc)
    {
        int length = data.GetTrailLength();
        IEnumerator<MoodSwing.MoodSwingTrailNode> nodes = data.GetTrail().GetEnumerator();
        int index = 0;
        //float currentYLerp = deltaLerp;
        while (nodes.MoveNext())
        {
            Vector3 top = nodes.Current.localPosTop, bot = nodes.Current.localPosBot;
            changeFunc?.Invoke(ref top, ref bot, index, length);
            MakeQuadFromLastPairToThisOne(ref index, top, bot, vertexData, triangleData);
        }
    }

    public static void MakeQuadFromLastPairToThisOne(ref int index, Vector3 top, Vector3 bot, List<Vector3> vertex, List<int> triangles)
    {
        if (index < 2)
        {
            vertex.Add(top);
            vertex.Add(bot);
            index += 2;
            return;
        }

        //There's the possibility to reuse quads
        vertex.Add(top);
        vertex.Add(bot);

        //Add triangle 1
        triangles.Add(index - 2);
        triangles.Add(index - 1);
        triangles.Add(index + 0);

        //Add triangle 2
        triangles.Add(index - 1);
        triangles.Add(index + 1);
        triangles.Add(index + 0);

        //Advance the pair
        index += 2;
    }
}
