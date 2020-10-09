using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttacheableArmature : MonoBehaviour
{
    public enum Part
    {
        None,
        Head,
        LeftHand,
        RightHand,
        Neck
    }

    public Transform head, leftHand, rightHand, neck;

    public void Attach(Transform model, Part part, Vector3 offset)
    {
        Transform p = GetPart(part);
        if(p != null)
        {
            Attach(model, p, offset);
        }
    }

    private void Attach(Transform model, Transform part, Vector3 offset)
    {
        model.SetParent(part, true);
        model.localPosition = offset;
    }

    public Transform GetPart(Part part)
    {
        switch(part)
        {
            
            case Part.Head:
                return head;
            case Part.LeftHand:
                return leftHand;
            case Part.RightHand:
                return rightHand;
            case Part.Neck:
                return neck;
            default:
                return null;
        }
    }
}