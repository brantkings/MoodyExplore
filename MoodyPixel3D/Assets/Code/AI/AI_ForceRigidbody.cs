using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AI_ForceRigibody : MonoBehaviour
{
    [Header("Force")]
    public Rigidbody body;
    public ForceMode mode = ForceMode.Force;
    public float forceMultiplier = 1f;
    public bool debug;

    private void Reset()
    {
        body = GetComponentInParent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        if (body == null) enabled = false;
        return;
    }

    protected void Force(Vector3 direction, float strength)
    {
        Force(direction.normalized * strength);
    }

    protected void Force(Vector3 force)
    {
#if UNITY_EDITOR
        if(debug)
            Debug.LogFormat("{0} adding {1} * {2} of force in {3}", this, force, forceMultiplier, mode);
#endif
        //if (mode == ForceMode.VelocityChange) force *= Time.fixedDeltaTime;
        Vector3 forceNew = force * forceMultiplier * Time.fixedDeltaTime;
        //Debug.LogFormat("force {0} * forceMultiplier {1} = {2} * fixedDeltaTime {3} = ({4},{5},{6})", force, forceMultiplier, force * forceMultiplier, Time.fixedDeltaTime, forceNew.x, forceNew.y, forceNew.z);
        body.AddForce(force * forceMultiplier, mode);
    }
}
