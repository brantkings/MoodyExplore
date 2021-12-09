using LHH.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mood.Swing.Maker
{
    public class MoodSwingMaker : MonoBehaviour
    {
        public Color gizmoColor = Color.red;
        public Color arrowColor = Color.magenta;
        public Color trailConnectColor = Color.yellow;
        public Color trailTopColor = Color.white;
        public Color trailBottomColor = Color.black;

        private MoodSwing.MoodSwingNode GetNode(Transform from)
        {
            return new MoodSwing.MoodSwingNode()
            {
                delay = 0f,
                localPosition = transform.TransformPoint(from.localPosition),
                radius = from.lossyScale.x,
                direction = from.forward
            };
        }

        private MoodSwing.MoodSwingTrailNode GetTrailNode(MoodSwingMakerTrailNode node)
        {
            if (node == null)
                return new MoodSwing.MoodSwingTrailNode();

            return new MoodSwing.MoodSwingTrailNode()
            {
                localPosTop = node.transform.TransformVector(node.TopNode.localPosition) + node.transform.localPosition,
                localPosBot = node.transform.TransformVector(node.BottomNode.localPosition) + node.transform.localPosition,
            };
        }


        private IEnumerable<Transform> KeepGettingChildren()
        {
            foreach (Transform t in NodeTransforms) yield return t;
            while(true)
            {
                GameObject o = new GameObject("[Helper]");
                o.transform.SetParent(transform);
                yield return o.transform;
            }
        }

        private int GetNodesCount()
        {
            int length = 0;
            foreach (Transform t in NodeTransforms)
            {
                length++;
            }
            return length;
        }

        private IEnumerable<MoodSwing.MoodSwingNode> GetAllNodes()
        {
            foreach(Transform t in NodeTransforms)
            {
                yield return GetNode(t);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.25f);
            Gizmos.color = gizmoColor;
            Color fillColor = gizmoColor * 0.25f;
            foreach(Transform t in NodeTransforms)
            {
                MoodSwing.MoodSwingNode node = GetNode(t);
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireSphere(transform.position + node.localPosition, node.radius);
                Gizmos.color = arrowColor;
                GizmosUtils.DrawArrow(transform.position + node.localPosition, transform.position + node.localPosition + node.direction, 0.5f);
                Gizmos.color = fillColor;
                Gizmos.DrawSphere(transform.position + node.localPosition, node.radius);
            }

            Transform oldTop = null, oldBot = null;

            foreach (MoodSwingMakerTrailNode t in GetAllTrailNodes())
            {
                Gizmos.color = trailConnectColor;
                Gizmos.DrawLine(t.TopNode.position, t.BottomNode.position);
                if(oldTop != null && oldBot != null)
                {
                    Gizmos.color = trailTopColor;
                    Gizmos.DrawLine(oldTop.position, t.TopNode.position);
                    Gizmos.color = trailBottomColor;
                    Gizmos.DrawLine(oldBot.position, t.BottomNode.position);
                }
                oldTop = t.TopNode;
                oldBot = t.BottomNode;

            }
        }

        private IEnumerable<Transform> NodeTransforms
        {
            get
            {
                foreach(Transform t in transform)
                {
                    if (t.GetComponent<MoodSwingMakerTrailNode>() != null) continue;
                    yield return t;
                }
            }
        }

        public IEnumerable<MoodSwing.MoodSwingNode> Nodes
        {
            get
            {
                foreach (Transform t in NodeTransforms) yield return GetNode(t);
            }
        }

        private MoodSwingMakerTrailNode[] _trailNodes = null;
        private MoodSwingMakerTrailNode[] TrailNodes
        {
            get
            {
                if(_trailNodes == null || _trailNodes.Length == 0)
                {
                    _trailNodes = GetAllTrailNodes();
                }
                return _trailNodes;
            }
        }

        private MoodSwingMakerTrailNode[] GetAllTrailNodes()
        {
            return transform.GetComponentsInChildren<MoodSwingMakerTrailNode>(true);
        }

        public IEnumerable<MoodSwing.MoodSwingTrailNode> Trail
        {
            get
            {
                for(int i = 0,len = TrailNodes.Length;i<len;i++)
                {
                    yield return GetTrailNode(TrailNodes[i]);
                }
            }
        }

        public int TrailLength
        {
            get
            {
                return TrailNodes.Length;
            }
        }
    }
}
