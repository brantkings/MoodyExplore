using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Caster
{
    [RequireComponent(typeof(Caster))]
    public class CasterTester : MonoBehaviour
    {
        Caster _toTest;

        public Vector3 distance;
        public Vector3 latestYellowPos;
        public int amountReflects;

        private void Awake()
        {
            _toTest = GetComponent<Caster>();
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;
            if (_toTest == null) _toTest = GetComponent<Caster>();
            int i = amountReflects;
            Vector3 originNow = _toTest.GetOriginPosition();
            Vector3 distanceNow = distance;
            RaycastHit oldHit = new RaycastHit();
            _toTest.DrawGizmoForCast(originNow, distanceNow.normalized, distanceNow.magnitude, out RaycastHit hit);
            while(i-- > 0 && hit.collider != null)
            {
                Vector3 hugWall = distanceNow.normalized * hit.distance;
                originNow += hugWall;
                distanceNow -= hugWall;
                distanceNow = Vector3.ProjectOnPlane(distanceNow, hit.normal);
                oldHit = hit;
                _toTest.DrawGizmoForCast(originNow, distanceNow.normalized, distanceNow.magnitude, out hit);

                Vector3 dist2 = _toTest.GetMinimumDistanceFromHit(hit.point, hit.normal);
                Vector3 dist1 = _toTest.GetMinimumDistanceFromHit(oldHit.point, oldHit.normal);
                Gizmos.color = Color.yellow;
                Vector3 lineIntersection = Vector3.Cross(oldHit.normal, hit.normal);
                Vector3 directionAlongsidePlaneHit = Vector3.Cross(hit.normal, lineIntersection);
                float numerator = Vector3.Dot(oldHit.normal, directionAlongsidePlaneHit);
                //Sum with the distances to actually get the intersection between the planes where the distances are already considering where the caster should be
                Vector3 plane1Pos = oldHit.point + dist1; 
                Vector3 plane2Pos = hit.point + dist2;
                Vector3 plane1ToPlane2 = plane1Pos - plane2Pos;
                float t = Vector3.Dot(oldHit.normal, plane1ToPlane2) / numerator; //Discover where does the planes intersect parametrically (I think? Need to study)
                Vector3 pointBetween = plane2Pos + t * directionAlongsidePlaneHit; // Go alonside the plane 2 with the direction to the intersection
                Vector3 yellowPos = pointBetween;// + Vector3.ClampMagnitude(dist1 + dist2, Mathf.Max(dist1.magnitude, dist2.magnitude));
                latestYellowPos = yellowPos;
                _toTest.DrawFormatGizmo(yellowPos);
            }
        }
        //Find the line of intersection between two planes.
        //The inputs are two game objects which represent the planes.
        //The outputs are a point on the line and a vector which indicates it's direction.
        void planePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, GameObject plane1, GameObject plane2)
        {
            linePoint = Vector3.zero;
            lineVec = Vector3.zero;

            //Get the normals of the planes.
            Vector3 plane1Normal = plane1.transform.up;
            Vector3 plane2Normal = plane2.transform.up;

            //We can get the direction of the line of intersection of the two planes by calculating the
            //cross product of the normals of the two planes. Note that this is just a direction and the
            //line is not fixed in space yet.
            lineVec = Vector3.Cross(plane1Normal, plane2Normal);

            //Next is to calculate a point on the line to fix it's position. This is done by finding a vector from
            //the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
            //errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
            //the cross product of the normal of plane2 and the lineDirection.  
            Vector3 ldir = Vector3.Cross(plane2Normal, lineVec);

            float numerator = Vector3.Dot(plane1Normal, ldir);

            //Prevent divide by zero.
            if (Mathf.Abs(numerator) > 0.000001f)
            {

                Vector3 plane1ToPlane2 = plane1.transform.position - plane2.transform.position;
                float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / numerator;
                linePoint = plane2.transform.position + t * ldir;
            }
        }

        /// <summary>
        /// Find the Closest point on a line to a given point in space
        /// </summary>
        /// <param name="vA">Point A of line</param>
        /// <param name="vB">Point B of line</param>
        /// <param name="vPoint">The point to measure shortest distance from</param>
        /// <returns>A point on given line where it is perpendicular to vPoint</returns>
        private Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
        {
            var vVector1 = vPoint - vA;
            var vVector2 = (vB - vA).normalized;

            var d = Vector3.Distance(vA, vB);
            var t = Vector3.Dot(vVector2, vVector1);

            if (t <= 0)
                return vA;

            if (t >= d)
                return vB;

            var vVector3 = vVector2 * t;

            var vClosestPoint = vA + vVector3;

            return vClosestPoint;
        }
    }

    
}
