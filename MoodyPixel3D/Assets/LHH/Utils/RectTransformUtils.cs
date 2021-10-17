using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Utils
{
    public static class RectTransformUtils
    {
        /// <summary>
        /// Didnt work for me!
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        public static Rect WorldRect(this RectTransform rectTransform)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
            float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

            Vector3 position = rectTransform.position;
            return new Rect(position.x - rectTransformWidth * 0.5f, position.y - rectTransformHeight * 0.5f, rectTransformWidth, rectTransformHeight);
        }

        public static void MakeRectGoToParentContext(this RectTransform rectTransform, ref Rect normalRect)
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            Rect parentRect = parent.rect;

            


            //Local position is the distance between parent pivot and this pivot
            //normalRect.position += (Vector2)(parent.localRotation * rectTransform.localPosition) * parent.localScale;

            normalRect.min += rectTransform.offsetMin;
        }

        public static bool OverlapsInSameCanvas(this RectTransform me, RectTransform other, RectTransform commonParent = null)
        {

            if (commonParent == null) commonParent = me.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            GetRectInRelationToParent(me, commonParent, out Rect myRect);
            GetRectInRelationToParent(other, commonParent, out Rect otherRect);

            return myRect.Overlaps(otherRect);
        }

        /// <summary>
        /// Get the point in the parent rect where RectTransform is anchored on. If you go to (0,0) of the anchoredPosition, you will be at this point.
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static Vector2 GetAnchorZero(this RectTransform me)
        {
            RectTransform parent = me.parent as RectTransform;
            if (parent != null)
            {
                Rect parentRect = parent.rect;
                return GetAnchorZero(me, parentRect);
            }
            else return -me.anchoredPosition;
            
        }

        private static Vector3 GetAnchorZero(RectTransform me, Rect parentRect)
        {
            Vector2 anchorMinPosition = new Vector2(Mathf.Lerp(parentRect.xMin, parentRect.xMax, me.anchorMin.x), Mathf.Lerp(parentRect.yMin, parentRect.yMax, me.anchorMin.y));
            Vector2 anchorMaxPosition = new Vector2(Mathf.Lerp(parentRect.xMin, parentRect.xMax, me.anchorMax.x), Mathf.Lerp(parentRect.yMin, parentRect.yMax, me.anchorMax.y));
            return Vector2.Lerp(anchorMinPosition, anchorMaxPosition, 0.5f);
        }

        public static void GetRectInRelationToParent(this RectTransform me, RectTransform parentAlongHierarchy, out Rect rect)
        {
            rect = me.rect;
            Debug.LogFormat("Starting as {0} with {1}", me, rect);
            while (me != parentAlongHierarchy && me != null)
            {
                RectTransform dad = me.parent as RectTransform;
                //Rect dadRect = dad.rect;
                rect.position = rect.position + (Vector2)me.localPosition;
                me = dad;
                Debug.LogFormat("Now I am {0} with {1}", me , rect);
            }
            Debug.LogFormat("Finished as {0} with {1}", me, rect);
        }

        /*public static bool Overlaps(this RectTransform rect, RectTransform other)
        {
            Vector3[] myCorners = new Vector3[4];
            Vector3[] otherCorners = new Vector3[4];

            rect.GetWorldCorners(myCorners);
            other.GetWorldCorners(otherCorners);

            


        }*/
    }
}