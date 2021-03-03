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
        public SkillDirectionSanitizer skillDirectionBeginning;
    }

    LHH.Unity.ComponentGetter<MeshFilter> filter;
    LHH.Unity.ComponentGetter<MeshRenderer> meshRenderer;
        
    private Mesh mesh;

    List<Vector3> vertexData = new List<Vector3>(16 * 4);
    List<int> triangleData = new List<int>(16 * 6);

    SkillDirectionSanitizer currentSanitizer;

    private void Start()
    {
        mesh = new Mesh();
        mesh.name = "RangeAreaMesh";
        filter.Get(gameObject).mesh = mesh;
    }


    public override void Show(MoodPawn pawn, Properties property)
    {
        currentSanitizer = property.skillDirectionBeginning;
        Debug.LogWarningFormat("{0} is {1} now", currentSanitizer, property.skillDirectionBeginning);

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

    public override void Hide(MoodPawn p)
    {
        meshRenderer.Get(gameObject).enabled = false;
        //Debug.LogFormat("Hiding {0} of {2} {1}", meshRenderer.Get(gameObject), meshRenderer.Get(gameObject).enabled, p.name);
    }

    public void SetDirection(MoodPawn pawn, MoodSkill skill, Vector3 directionLength)
    {
        transform.position = pawn.UsedCurrentSkill() || skill != pawn.GetCurrentSkill()? pawn.Position : pawn.GetSkillPreviewOriginPosition();
        transform.forward = directionLength;
        transform.localPosition += transform.parent.InverseTransformVector(currentSanitizer.Sanitize(directionLength, pawn.Direction));
        //transform.localPosition = transform.parent.InverseTransformVector(directionLength);
    }
}
