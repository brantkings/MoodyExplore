using LHH.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MoodSwingMaker : MonoBehaviour
{
    public MoodSwing from;
    public MoodSwing to;

    public Color gizmoColor = Color.red;
    public Color arrowColor = Color.magenta;
    public Color trailConnectColor = Color.yellow;
    public Color trailColor = Color.white;

    private void SetTransform(Transform to, MoodSwing.MoodSwingNode from)
    {
        //to.delay = 0f;
        to.localPosition = from.localPosition;
        float scale = from.radius;
        to.localScale = new Vector3(scale, to.localScale.y, to.localScale.z);
        to.forward = from.direction;
    }

    private MoodSwing.MoodSwingNode GetNode(Transform from)
    {
        return new MoodSwing.MoodSwingNode()
        {
            delay = 0f,
            localPosition = from.localPosition,
            radius = from.localScale.x,
            direction = from.forward
        };
    }
    
    [LHH.Unity.Button]
    public void MakeChildrenFrom()
    {
        foreach(Transform t in Nodes)
        {
            DestroyImmediate(t.gameObject);
        }
        IEnumerator<Transform> children = KeepGettingChildren().GetEnumerator();
        foreach (MoodSwing.MoodSwingNode node in from.data)
        {
            if (!children.MoveNext()) break;
            Transform t = children.Current;
            SetTransform(t, node);
        }
    }

    [LHH.Unity.Button]
    public void BakeChildrenTo()
    {
        to.SetData(GetAllNodes(), GetNodesCount());
    }


    private IEnumerable<Transform> KeepGettingChildren()
    {
        foreach (Transform t in Nodes) yield return t;
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
        foreach (Transform t in Nodes)
        {
            length++;
        }
        return length;
    }

    private IEnumerable<MoodSwing.MoodSwingNode> GetAllNodes()
    {
        foreach(Transform t in Nodes)
        {
            yield return GetNode(t);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Color fillColor = gizmoColor * 0.25f;
        foreach(Transform t in Nodes)
        {
            MoodSwing.MoodSwingNode node = GetNode(t);
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position + node.localPosition, node.radius);
            Gizmos.color = arrowColor;
            GizmosUtils.DrawArrow(transform.position + node.localPosition, transform.position + node.localPosition + node.direction);
            Gizmos.color = fillColor;
            Gizmos.DrawSphere(transform.position + node.localPosition, node.radius);
        }

        Transform oldTop = null, oldBot = null;

        foreach (MoodSwingMakerTrailNode t in Trail)
        {
            Gizmos.color = trailConnectColor;
            Gizmos.DrawLine(t.TopNode.position, t.BottomNode.position);
            if(oldTop != null && oldBot != null)
            {
                Gizmos.color = trailColor;
                Gizmos.DrawLine(oldTop.position, t.TopNode.position);
                Gizmos.DrawLine(oldBot.position, t.BottomNode.position);
            }

        }
    }

    private IEnumerable<Transform> Nodes
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

    private IEnumerable<MoodSwingMakerTrailNode> Trail
    {
        get
        {
            return transform.GetComponentsInChildren<MoodSwingMakerTrailNode>();
        }
    }
}
