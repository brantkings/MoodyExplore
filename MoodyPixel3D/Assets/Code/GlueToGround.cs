using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlueToGround : MonoBehaviour
{
    public Transform planePosition;
    public Transform raycastOrigin;
    public LayerMask groundLayer;
    public float maxRaycast;
    public float sphereRadius = 0.5f;
    public Renderer graphic;


    Vector3 _lastPos;
    Ray _ray;

    private Vector3 Origin
    {
        get
        {
            if (raycastOrigin != null) return raycastOrigin.position;
            else if (transform.parent != null) return transform.parent.position;
            else return transform.position;
        }
    }

    private Vector3 PlanePosition
    {
        get
        {
            if (planePosition != null) return planePosition.position;
            return transform.position;
        }
    }


    private void Awake()
    {
        _ray = new Ray(Origin, -Vector3.up);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(Origin, Origin - Vector3.up * 6f);
        if(Raycast(Origin - Vector3.up * 2, out RaycastHit hit))
        {
            Gizmos.DrawCube(hit.point + hit.normal * 0.02f, new Vector3(1f, 0.02f, 1f));
        }
    }

    private void LateUpdate()
    {
        Vector3 pos = Origin;
        //if (pos != _lastPos) Glue(pos);
        Glue(pos);

        _lastPos = pos;
    }

    private bool Raycast(Vector3 pos, out RaycastHit hit)
    {
        _ray.origin = pos - _ray.direction;
        return Physics.SphereCast(_ray, sphereRadius, out hit, maxRaycast + 1, groundLayer.value, QueryTriggerInteraction.Ignore);
    }

    private void Glue(Vector3 pos)
    {
        RaycastHit hit;
        if(Raycast(pos, out hit))
        {
            transform.position = new Vector3(PlanePosition.x, hit.point.y + 0.1f, PlanePosition.z);
            transform.up = hit.normal;
            if (transform.parent != null)
            {
                transform.forward = transform.parent.forward;
                transform.right = transform.parent.right;
            }
            graphic.enabled = true;
        }
        else
        {
            graphic.enabled = false;
        }
    }
}
