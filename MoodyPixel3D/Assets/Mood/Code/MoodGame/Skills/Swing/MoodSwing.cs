using FMOD;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mood.Swing.Maker;


[CreateAssetMenu(menuName ="Mood/Swing Data", fileName = "Swing_")]
public class MoodSwing : ScriptableObject
{
    public static int CACHE_SIZE = 8;

    public struct MoodSwingBuildData
    {
        private static Collider[] _colliderCache = new Collider[CACHE_SIZE];
        private static Dictionary<Collider, MoodSwingResult> _resultsCache = new Dictionary<Collider, MoodSwingResult>(CACHE_SIZE);

        public MoodSwing moodSwingItself;
        public Vector3 localOffset;

        public MoodSwingBuildData(MoodSwing m)
        {
            moodSwingItself = m;
            localOffset = Vector3.zero;
        }

        public MoodSwingBuildData AddOffset(Vector3 offset)
        {
            localOffset = offset;
            return this;
        }

        public MoodSwingResult? TryHitGetBest(Vector3 posOrigin, Quaternion rotOrigin, LayerMask layer, Vector3 desiredDirection)
        {
            float minAngle = float.MaxValue;
            MoodSwingResult? result = null;
            foreach (MoodSwingResult r in TryHitMerged(posOrigin, rotOrigin, layer))
            {
                Vector3 direction = r.collider.ClosestPoint(posOrigin) - posOrigin;
                float angle = Vector3.Angle(direction, desiredDirection);
                if (angle < minAngle)
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
            foreach (MoodSwingNode node in moodSwingItself.maker.Nodes)
            {
                LHH.Utils.DebugUtils.DrawCircle(GetCorrectPosition(posOrigin, rotOrigin, localOffset, node), node.radius, rotOrigin * Vector3.up, Color.black, 1f);
                int result = Physics.OverlapSphereNonAlloc(GetCorrectPosition(posOrigin, rotOrigin, localOffset, node), node.radius, _colliderCache, layer.value, QueryTriggerInteraction.Collide);
                if (result > 0)
                {
                    for (int j = 0, lenA = Mathf.Min(result, CACHE_SIZE); j < lenA; j++)
                    {
                        return GetResultFrom(posOrigin, rotOrigin, localOffset, node, _colliderCache[j]);
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
            int b = 0;
            foreach (MoodSwingNode node in moodSwingItself.maker.Nodes)
            {
                LHH.Utils.DebugUtils.DrawCircle(GetCorrectPosition(posOrigin, rotOrigin, localOffset, node), node.radius, rotOrigin * Vector3.up, Color.black, 1f);
                int result = Physics.OverlapSphereNonAlloc(GetCorrectPosition(posOrigin, rotOrigin, localOffset, node), node.radius, _colliderCache, layer.value, QueryTriggerInteraction.Collide);
                if (result > 0)
                {
                    for (int j = 0, lenA = Mathf.Min(result, CACHE_SIZE); j < lenA; j++)
                    {
                        Collider collider = _colliderCache[j];
                        MoodSwingResult newResult = GetResultFrom(posOrigin, rotOrigin, localOffset, node, collider);
                        UnityEngine.Debug.LogFormat("[SWING] MoodSwing {0}: {1} (from {2}) collider is found. Has collider? {3} ({4} iteration)", 
                            layer.value, collider.name, collider.GetComponentInParent<MoodPawn>()?.name, _resultsCache.ContainsKey(collider), ++b);
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

        public float GetRange()
        {
            float range = 0f;
            foreach (var d in moodSwingItself.maker.Nodes)
            {
                range = Mathf.Max(range, Vector3.ProjectOnPlane(d.localPosition, Vector3.up).magnitude + d.radius);
            }
            return range + localOffset.magnitude;
        }
    }

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


    public IEnumerable<MoodSwingTrailNode> GetTrail()
    {
        return maker.Trail;
    }

    public int GetTrailLength()
    {
        return maker.TrailLength;
    }

    public MoodSwingBuildData GetBuildData(Vector3 direction, Vector3 offset = default)
    {
        return GetBuildData(Quaternion.LookRotation(direction, Vector3.up), offset);
    }
    public MoodSwingBuildData GetBuildData(MoodPawn originalPawn, Vector3 offset = default)
    {
        return GetBuildData(originalPawn.ObjectTransform.rotation, offset);
    }

    public MoodSwingBuildData GetBuildData(Quaternion objectData, Vector3 offset = default)
    {
        return new MoodSwingBuildData(this).AddOffset(offset);
        //return new MoodSwingBuildData(this).AddOffset(objectData * offset);
    }

    private static Vector3 GetCorrectPosition(Vector3 posOrigin, Quaternion rotOrigin, Vector3 posOffset, MoodSwingNode node)
    {
        return posOrigin + rotOrigin * (node.localPosition + posOffset);
    }


    private static MoodSwingResult GetResultFrom(Vector3 posOrigin, Quaternion rotOrigin, Vector3 posOffset, MoodSwingNode node, Collider col)
    {
        return new MoodSwingResult()
        {
            hitDirection = rotOrigin * node.direction,
            hitPosition = Vector3.Lerp(GetCorrectPosition(posOrigin, rotOrigin, posOffset, node), col.transform.position, 0.5f),
            collider = col
        };

    }


    public delegate void DelUpdateVectors(ref Vector3 top, ref Vector3 bot, int index, int length);

    public static void MakeVertexTrailRightToLeft(MoodSwingBuildData data, List<Vector3> vertexData, List<int> triangleData, DelUpdateVectors changeFunc)
    {
        int length = data.moodSwingItself.GetTrailLength();
        IEnumerator<MoodSwing.MoodSwingTrailNode> nodes = data.moodSwingItself.GetTrail().GetEnumerator();
        int index = 0;
        //float currentYLerp = deltaLerp;
        UnityEngine.Debug.LogFormat("[MOODSWING] Building {0} with {1}.", data.moodSwingItself, data.localOffset);
        while (nodes.MoveNext())
        {
            Vector3 top = nodes.Current.localPosTop + data.localOffset, bot = nodes.Current.localPosBot + data.localOffset;
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
