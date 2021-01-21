using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGizmo : MonoBehaviour
{
    public Transform from;
    public Transform to;
    public Vector3 perpendicularXRotation;
    public float perpendicularXDistance = 0.5f;

    public Color gizmoSelected = Color.white;
    public Color gizmoUnselected = Color.black;

    private Vector3 From
    {
        get
        {
            if (from != null) return from.position;
            else return transform.position;
        }
    }

    private Vector3 To
    {
        get
        {
            if (to != null) return to.position;
            else return transform.position;
        }
    }

    private void OnDrawGizmos()
    {
        DrawGizmos(gizmoUnselected);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos(gizmoSelected);
    }

    private void DrawGizmos(Color c)
    {
        Gizmos.color = c;
        Vector3 dist = From - To;
        Vector3 perpDist = (Quaternion.Euler(perpendicularXRotation) * dist).normalized * perpendicularXDistance;
        Gizmos.DrawLine(From + perpDist, To - perpDist);
        Gizmos.DrawLine(From - perpDist, To + perpDist);
    }
}
