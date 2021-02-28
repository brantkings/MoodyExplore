using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeTarget : RangeShow<RangeTarget.Properties>
{
    [System.Serializable]
    public class Properties
    {
        public float radiusAround;
        public Transform target;
    }

    public Transform targetRender;
    private Properties _properties;

    private bool _showing;
    
    
    public override void Show(MoodPawn pawn,  Properties properties)
    {
        _properties = properties;
        _showing = true;
    }

    private void Update()
    {
        if (_properties != null && _properties.target != null)
        {
            targetRender.gameObject.SetActive(_showing);
            targetRender.position = _properties.target.position;
            targetRender.rotation = _properties.target.rotation;   
        }
        else
        {
            
            targetRender.gameObject.SetActive(false); 
        }
    }

    public override void Hide(MoodPawn pawn)
    {
        targetRender.gameObject.SetActive(false);
        _showing = false;
    }
}
