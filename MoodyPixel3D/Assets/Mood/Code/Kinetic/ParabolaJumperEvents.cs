using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParabolaJumperEvents : AddonBehaviour<OldParabolaJumper>
{
    public UnityEvent OnJump;
    public UnityEvent OnStopJump;

    private void OnEnable()
    {
        Addon.OnPlatformerGroundedStateChange += OnChange;
    }
    private void OnDisable()
    {
        Addon.OnPlatformerGroundedStateChange -= OnChange;
    }

    private void OnChange(IPlatformer plat, GroundedState what)
    {
        switch (what)
        {
            case GroundedState.Neither:
                OnStopJump.Invoke();
                break;
            case GroundedState.Grounded:
                break;
            case GroundedState.Aerial:
                OnJump.Invoke();
                break;
            default:
                break;
        }
    }
}
