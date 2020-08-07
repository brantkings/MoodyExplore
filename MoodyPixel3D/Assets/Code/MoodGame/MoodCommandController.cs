using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;

public class MoodCommandController : MonoBehaviour
{
    private RangeSphere _rangeIndicator;
    [SerializeField]
    private Canvas _canvas;
    
    private void Awake()
    {
        _rangeIndicator = GetComponentInChildren<RangeSphere>();

        Deactivate();
    }

    public void Activate(Vector3 position, float radius)
    {
        transform.position = position;
        SetActiveObjects(true, radius);
    }

    public void Deactivate()
    {
        SetActiveObjects(false, 0f);

    }
    

    private void SetActiveObjects(bool active, float radius)
    {
        if (active)
            _rangeIndicator.Show(radius);
        else
            _rangeIndicator.Hide();
        
        _canvas.gameObject.SetActive(active);
    }
    
}
