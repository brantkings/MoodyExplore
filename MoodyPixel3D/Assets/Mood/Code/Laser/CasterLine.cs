using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;

public class CasterLine : MonoBehaviour
{
    public Caster caster;
    public Transform origin;
    public Transform endPoint;

    private void Update()
    {
        origin.position = caster.GetOriginPosition();
        origin.forward = caster.GetDefaultDirectionNormalized();
        int i = 0;
        foreach(RaycastHit hit in caster.CastAll())
        {
            if(hit.collider != null)
            {
                i++;
                if(hit.distance != 0f)
                {
                    endPoint.position = hit.point;
                    endPoint.forward = hit.normal;
                    endPoint.gameObject.SetActive(true);
                }
                else
                {
                    endPoint.position = origin.position;
                    endPoint.forward = -origin.forward;
                    endPoint.gameObject.SetActive(false);
                }
            }
        }
        if(i==0)
        {
            endPoint.position = caster.GetOriginPosition() + caster.GetDefaultDirectionNormalized() * caster.GetDefaultDistance();
            endPoint.forward = -caster.GetDefaultDirectionNormalized();
        }
    }

}
