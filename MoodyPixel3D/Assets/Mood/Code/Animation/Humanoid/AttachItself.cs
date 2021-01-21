using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachItself : MonoBehaviour
{
    public AttacheableArmature.Part part;
    [Header("Leave it null it will try to find in parent.")]
    public AttacheableArmature whereToAttach;

    public bool cancelSelfRotation;

    public Vector3 localPosition;

    public bool attachOnStart;

    private void Awake() {
        if(whereToAttach == null)
        {
            FindWhereToAttach();
        }   
    }

    private void Start() {
        if(attachOnStart) Attach();
    }

    public void FindWhereToAttach()
    {
        whereToAttach = GetComponentInParent<AttacheableArmature>();
    }

    public void Attach()
    {
        if(whereToAttach != null) whereToAttach.Attach(transform, part, localPosition);
        if(cancelSelfRotation) this.transform.localRotation = Quaternion.identity;
    }
}
