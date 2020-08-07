using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArea
{
    Vector3 GetRandomPosition();
}

public abstract class Area : MonoBehaviour, IArea
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        DrawGizmo();
    }

    public abstract void DrawGizmo();

    public abstract Vector3 GetRandomPosition();
}
