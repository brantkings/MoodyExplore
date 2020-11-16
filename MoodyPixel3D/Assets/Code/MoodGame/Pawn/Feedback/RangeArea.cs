using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class RangeArea : RangeShow<RangeArea.Properties>, IRangeShowDirected
{
    public struct Properties
    {
        public MoodSwing swingData;
    }

    LHH.Unity.ComponentGetter<MeshFilter> filter;
    LHH.Unity.ComponentGetter<MeshRenderer> meshRenderer;
        
    private Mesh mesh;

    List<Vector3> vertexData = new List<Vector3>(16 * 4);
    List<int> triangleData = new List<int>(16 * 6);

    private void Start()
    {
        mesh = new Mesh();
        mesh.name = "RangeAreaMesh";
        filter.Get(gameObject).mesh = mesh;
    }


    public override void Show(Properties property)
    {
        vertexData.Clear();
        triangleData.Clear();
        MoodSwing.MakeVertexTrailRightToLeft(property.swingData, vertexData, triangleData, GetYRight);
        mesh.Clear();
        mesh.SetVertices(vertexData);
        mesh.SetTriangles(triangleData, 0);
        mesh.RecalculateBounds();
        meshRenderer.Get(gameObject).enabled = true;
    }

    private void GetYRight(ref Vector3 top, ref Vector3 bot, int index, int length)
    {
        top.y = 0.05f;
        bot.y = 0.05f;
    }

    public override void Hide()
    {
        meshRenderer.Get(gameObject).enabled = false;
    }

    public void SetDirection(Vector3 directionLength)
    {
        transform.forward = directionLength;
    }
}
