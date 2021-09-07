using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicPlatformerCollisionDetection : MonoBehaviour
{
    KinematicPlatformer _plat;
    public int bufferSize = 8;

    private ContactPoint[] buffer;

    private void Awake()
    {
        _plat = GetComponentInChildren<KinematicPlatformer>();
        if(_plat == null)
        {
            Debug.LogErrorFormat(this, "Can't find a kinematic platformer under {0}", this);
            enabled = false;
            return;
        }
        buffer = new ContactPoint[bufferSize];
    }

    private void OnCollisionStay(Collision collision)
    {
        int len = collision.GetContacts(buffer);
        for(int i = 0;i<len;i++)
        {
            ContactPoint point = buffer[i];
            Vector3 direction = Vector3.ProjectOnPlane(-point.normal, Vector3.up);
            if(direction != Vector3.zero)
            {
                //_plat.AddArbitraryCheckConsumable(direction, point.separation, KinematicPlatformer.CasterClass.Side);
                Rigidbody otherBody = point.otherCollider.GetComponentInParent<Rigidbody>();
                Vector3? extraValue = otherBody?.velocity * Time.fixedDeltaTime;

                _plat.CheckSurfaceNow(KinematicPlatformer.CasterClass.Side, Vector3.zero, direction, 0f, extraValue.HasValue? extraValue.Value : Vector3.zero);
                /*Debug.LogFormat(otherBody != null? (Object)otherBody : this, "Collision added {0} {1} between {2} and {3}. Distance is {4}. Other body is {5} with velocity {6}", 
                    point.normal, point.point, point.thisCollider, point.otherCollider, point.separation,
                    otherBody, extraValue?.ToString("F3"));*/
            }
        }
    }

}
