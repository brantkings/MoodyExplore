using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxArea : Area
{
    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private Vector3 size;

    

    public override Vector3 GetRandomPosition()
    {
        Vector3 size = GetSize();
        return GetOrigin() + new Vector3(
            Random.Range(-size.x, size.x) * 0.5f,
            Random.Range(-size.y, size.y) * 0.5f,
            Random.Range(-size.z, size.z) * 0.5f
            );
    }

    public Vector3 GetOrigin()
    {
        return transform.TransformPoint(offset);
    }

    public Vector3 GetSize()
    {
        return transform.TransformVector(size);
    }

    public override void DrawGizmo()
    {
        Gizmos.DrawWireCube(GetOrigin(), GetSize());
    }
}
