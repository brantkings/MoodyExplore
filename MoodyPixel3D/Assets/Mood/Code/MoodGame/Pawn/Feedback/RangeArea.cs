using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class RangeArea : RangeShow<RangeArea.Properties>, IRangeShowDirected
{
    public struct Properties
    {
        public enum Positioning
        {
            CurrentPosition,
            DashingDestination,
            OriginalPosition,
            OriginalPositionPlusDirection,
        }

        public MoodSwing swingData;
        public Vector3 offset;
        public SkillDirectionSanitizer skillPreviewSanitizer;
        public Positioning positioningWhenUsingSkill;
    }

    LHH.Unity.ComponentGetter<MeshFilter> filter;
    LHH.Unity.ComponentGetter<MeshRenderer> meshRenderer;
        
    private Mesh mesh;

    List<Vector3> vertexData = new List<Vector3>(16 * 4);
    List<int> triangleData = new List<int>(16 * 6);

    SkillDirectionSanitizer currentPositionSanitized;
    Properties.Positioning whereToPosition;
    private Vector3? directionAtPreview;

    private void Start()
    {
        mesh = new Mesh();
        mesh.name = "RangeAreaMesh";
        filter.Get(gameObject).mesh = mesh;
    }


    public override void Show(MoodPawn pawn, Properties property)
    {
        currentPositionSanitized = property.skillPreviewSanitizer;
        whereToPosition = property.positioningWhenUsingSkill;
        Debug.LogWarningFormat("{0} is {1} now", currentPositionSanitized, property.skillPreviewSanitizer);

        vertexData.Clear();
        triangleData.Clear();
        MoodSwing.MakeVertexTrailRightToLeft(property.swingData.GetBuildData(pawn.ObjectTransform.rotation, property.offset), vertexData, triangleData, GetYRight);
        mesh.Clear();
        mesh.SetVertices(vertexData);
        mesh.SetTriangles(triangleData, 0);
        mesh.RecalculateBounds();
        meshRenderer.Get(gameObject).enabled = true;
        Debug.LogWarningFormat(this, "Hey, {0} just showed {1}!", pawn?.name, this);
    }

    private void GetYRight(ref Vector3 top, ref Vector3 bot, int index, int length)
    {
        top.y = 0.05f;
        bot.y = 0.05f;
    }

    public override void Hide(MoodPawn p)
    {
        meshRenderer.Get(gameObject).enabled = false;
        //Debug.LogWarningFormat(this, "Hey, {0} just hide {1}!", p?.name, this);
    }

    public void SetDirection(MoodPawn pawn, MoodSkill skill, Vector3 directionLength)
    {
        Vector3 sanitizedDirection = currentPositionSanitized.Sanitize(directionLength, pawn.Direction);
        //transform.forward = directionLength;
        string debugType;
        if (pawn.UsedCurrentSkill() || skill != pawn.GetCurrentSkill()) //If it is not using a skill then it is previewing
        {
            debugType = "Preview";
            transform.position = pawn.Position;
            transform.localPosition += transform.parent.InverseTransformVector(sanitizedDirection); 
            transform.forward = directionLength;

            directionAtPreview = sanitizedDirection;
        }
        else //Then use its own position
        {
            switch (whereToPosition)
            {
                case Properties.Positioning.CurrentPosition:
                    debugType = "OwnPosition";
                    transform.position = pawn.Position;
                    break;
                case Properties.Positioning.DashingDestination:
                    if(pawn.IsDashing())
                    {
                        debugType = "Dashing";
                        transform.position = pawn.GetCurrentDashData().endPosition;
                    }
                    else
                    {
                        debugType = "TriedDashing";
                        transform.position = pawn.Position;
                    }
                    break;
                case Properties.Positioning.OriginalPositionPlusDirection:
                    debugType = "OriginalPositionPlusDirection";
                    transform.position = pawn.GetSkillPreviewOriginPosition();
                    transform.localPosition += transform.parent.InverseTransformVector(sanitizedDirection); 
                    transform.forward = directionLength;
                    break;
                case Properties.Positioning.OriginalPosition:
                    debugType = "OriginalPosition";
                    transform.position = pawn.GetSkillPreviewOriginPosition();
                    if(directionAtPreview.HasValue)
                        transform.localPosition += transform.parent.InverseTransformVector(directionAtPreview.Value);
                    break;
                default:
                    debugType = "DefaultedToPosition";
                    transform.position = pawn.Position;
                    break;
            }

            //transform.position = pawn.GetSkillPreviewOriginPosition();
            //transform.localPosition += transform.parent.InverseTransformVector(currentSanitizer.Sanitize(directionLength, pawn.Direction));
        }

        //Debug.LogFormat("Set direction with {0}, length {1} (Current for {2} is {3}). {4}: Position is {5} Direction is {6}", skill, directionLength, pawn, pawn.GetCurrentSkill(), debugType, transform.position, transform.forward);


        /*Debug.LogErrorFormat("{0} is Used? {1} or Skill different than current? {2}. Pawn Position is {3} while OriginPosition is {4} ({5})", 
            transform.position, pawn.UsedCurrentSkill(), skill != pawn.GetCurrentSkill(), pawn.Position, pawn.GetSkillPreviewOriginPosition(), this);*/
        //transform.localPosition = transform.parent.InverseTransformVector(directionLength);
    }

}
