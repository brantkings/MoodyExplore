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
        foreach(Transform t in transform)
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
        to.SetData(GetAllNodes(), GetAllNodesLength());
    }


    private IEnumerable<Transform> KeepGettingChildren()
    {
        foreach (Transform t in transform) yield return t;
        while(true)
        {
            GameObject o = new GameObject("[Helper]");
            o.transform.SetParent(transform);
            yield return o.transform;
        }
    }


    private IEnumerable<MoodSwing.MoodSwingNode> GetAllNodes()
    {
        foreach(Transform t in transform) 
            yield return GetNode(t);
    }

    private int GetAllNodesLength()
    {
        return transform.childCount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Color fillColor = gizmoColor * 0.25f;
        foreach(Transform t in transform)
        {
            MoodSwing.MoodSwingNode node = GetNode(t);
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position + node.localPosition, node.radius);
            Gizmos.color = arrowColor;
            GizmosUtils.DrawArrow(transform.position + node.localPosition, transform.position + node.localPosition + node.direction);
            Gizmos.color = fillColor;
            Gizmos.DrawSphere(transform.position + node.localPosition, node.radius);
        }
    }
}
