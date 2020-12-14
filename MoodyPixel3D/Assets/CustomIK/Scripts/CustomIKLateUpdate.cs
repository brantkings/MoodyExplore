using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomIKLateUpdate : CustomIK
{
    public override void Update()
    {
        //Do nothing here!
    }

    private void LateUpdate()
    {
        base.Update();
    }
}
