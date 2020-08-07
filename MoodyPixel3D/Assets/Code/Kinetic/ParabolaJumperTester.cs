using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParabolaJumper))]
public class ParabolaJumperTester : AddonBehaviour<ParabolaJumper>
{
    public KinematicPlatformer Body;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Addon.Jump();
        }
    }
}
