using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_RotateToTarget_OnEnable : AI_RotateToTarget_Core
{
    private void OnEnable()
    {
        RotateIfDetecting();
    }
}
