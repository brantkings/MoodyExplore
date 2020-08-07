using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerTriggerDetector : Detector
{
    public enum Situation
    {
        Grounded,
        NotGrounded,
        Any,
        None
    }

    private KinematicPlatformer platformerChecking;

    [Space()]
    public Situation whenIsDetected = Situation.Grounded;


    private void OnEnable()
    {
        CheckPlatformer();
    }

    private void OnDisable()
    {
        platformerChecking = null;
        CheckPlatformer();
    }

    private void OnTriggerEnter(Collider other)
    {
        KinematicPlatformer plat = other.GetComponentInParent<KinematicPlatformer>();
        if (plat != null) AddPlatformer(plat);
    }

    private void OnTriggerExit(Collider other)
    {
        KinematicPlatformer plat = other.GetComponentInParent<KinematicPlatformer>();
        if (plat != null) RemovePlatformer(plat);
    }

    private void AddPlatformer(KinematicPlatformer plat)
    {
        if (platformerChecking != plat)
        {
            UpdateTarget(plat.transform);
            platformerChecking = plat;
            platformerChecking.Grounded.OnChanged += OnStateChange;
        }
        CheckPlatformer();
    }

    private void OnStateChange(bool change)
    {
        TryUpdateDetecting(ShouldBeOn(change));
    }

    private void RemovePlatformer(KinematicPlatformer plat)
    {
        if (platformerChecking != null && !platformerChecking.Equals(null))
            platformerChecking.Grounded.OnChanged -= OnStateChange;
        platformerChecking = null;
        CheckPlatformer();
    }

    private bool CheckPlatformer()
    {
        if (platformerChecking != null && !platformerChecking.Equals(null))
        {
            TryUpdateDetecting(ShouldBeOn(platformerChecking.Grounded));
        }
        return false;
    }

    public bool ShouldBeOn(bool state)
    {
        switch (whenIsDetected)
        {
            case Situation.Grounded:
                return state;
            case Situation.NotGrounded:
                return !state;
            case Situation.Any:
                return true;
            case Situation.None:
                return false;
            default:
                return state;
        }
    }
}
