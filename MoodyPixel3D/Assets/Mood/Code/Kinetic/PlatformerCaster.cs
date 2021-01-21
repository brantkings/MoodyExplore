using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;

public class PlatformerCaster : MonoBehaviour, IPlatformer
{
    [SerializeField]
    private Caster _groundCaster;

    [SerializeField]
    private Transform _feetPosition;

    private bool _grounded;
    private Vector3 _lastPoint;
    private Vector3 _lastNormal;
    private int _lastCheckFrame;

    public event DelPlatformerConditionalEvent OnPlatformerGroundedStateChange;

    public bool Cast(Vector3 direction, float distance, out RaycastHit hit)
    {
        return _groundCaster.CastLength(direction.normalized * distance, out hit);
    }

    protected LayerMask GroundLayer
    {
        get
        {
            return _groundCaster.HitMask;
        }
    }

    public virtual bool NeedExternalCheck
    {
        get
        {
            return true;
        }
    }

    public GroundedState IsGrounded()
    {
        if (_grounded) return GroundedState.Grounded;
        else return GroundedState.Aerial;
    }

    public Vector3 RelativeFeetPosition
    {
        get
        {
            if (_feetPosition == null) return Vector3.zero;
            else
            {
                Vector3 relativeVec = _feetPosition.position - transform.position;
                relativeVec.x = relativeVec.z = 0f;
                return relativeVec;
            }
        }
    }

    public Vector3 LastRelativeGroundPosition
    {
        get
        {
            return GetGroundedPosition(_lastPoint);
        }
    }

    /// <summary>
    /// Get the position this transform should be if it was grounded in position pos.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector3 GetGroundedPosition(Vector3 pos)
    {
        //Debug.LogFormat("Getting grounded position for {0}: {1} - {2} = {3}", this, pos.y, RelativeFeetPosition.y, (pos - RelativeFeetPosition).y);
        return pos - RelativeFeetPosition;
    }

    public bool Cast()
    {
        int frame = Time.frameCount;
        if (_lastCheckFrame == frame) return _grounded;

        RaycastHit info;
        _lastCheckFrame = frame;
        bool grounded = _groundCaster.Cast(-Vector3.up, out info);
        SetGrounded(grounded, info.point, info.normal);
        return _grounded;
    }

    protected void SetGrounded(bool grounded, Vector3 point, Vector3 normal)
    {
        bool changed = grounded != _grounded;
        _grounded = grounded;
        if (_grounded)
        {
            _lastNormal = normal;
            _lastPoint = point;
        }
        if (changed && OnPlatformerGroundedStateChange != null) OnPlatformerGroundedStateChange(this, IsGrounded());
    }
    public Vector3 GetGroundedPositionOfLatestCheck()
    {
        return GetGroundedPosition(_lastPoint);
    }


    public void CheckPlatform()
    {
        Cast();
    }
}
