using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamePositionOnEnable : SamePosition
{
    private void OnEnable()
    {
        SetLikeTarget();
    }
}
