using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolaJumperFreefaller : ParabolaJumper
{
    private float _freefallCount = 0f;

    private JumpData _freefallData;

    Vector3 _positionBefore;
    Vector3 _positionNow;

    private void Start()
    {
        BeginFreefall();
    }

    public void BeginFreefall()
    {
        _freefallData = GetJumpDataWithThisStartingPosition(Body.AbsoluteVelocity);
        _freefallCount = 0f;

        _positionBefore = GetParabolaPositionOnTime(_freefallCount, _freefallData, HalfParabolaNoXFunction);
        _positionNow = _positionBefore;
    }

    protected override void EndJump()
    {
        base.EndJump();
        BeginFreefall();
    }

    private void FixedUpdate()
    {
        if (!IsDoingJumpRoutine())
        {
            if (IsGrounded())
            {
                BeginFreefall();
                //AdvanceTime(ref _freefallCount);
            }

            if (!IsEndOfJump(_freefallCount))
            {
                AdvanceTime(ref _freefallCount);
                _positionBefore = _positionNow;
                _positionNow = GetParabolaPositionOnTime(_freefallCount, _freefallData, HalfParabolaNoXFunction);
                Advance(_positionNow, _positionBefore);
            }
        }
        else
        {
        }
    }
    protected Vector2 HalfParabolaNoXFunction(float t)
    {
        float dislocatedT = (t) * 2f;
        return new Vector2(0f, 1f - Mathf.Pow(dislocatedT, jumpFormulaExponent));
    }
}
