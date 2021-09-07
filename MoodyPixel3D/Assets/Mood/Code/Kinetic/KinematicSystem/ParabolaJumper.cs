using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LHH.Utils;

public class ParabolaJumper : MonoBehaviour
{
    private static string JUMPER_ID = "ParJumper";

    public struct JumpData
    {
        public Vector3 origin;
        public Quaternion orientation;
        public float jumpDistance;
        public Vector3 jumpDirection;

        public JumpData(Transform from, Vector3 direction)
        {
            origin = from.position;
            orientation = from.rotation;
            jumpDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            jumpDistance = direction.magnitude;
        }

        public JumpData(Vector3 pos, Quaternion rot, Vector3 direction)
        {
            origin = pos;
            orientation = rot;
            jumpDirection = direction;
            jumpDistance = jumpDirection.magnitude;
        }

        public override string ToString()
        {
            return string.Format("Distance is {2} while direction vector is {3}, Orientation Euler is {1} and origin is {0}.", origin, orientation.eulerAngles, jumpDistance, jumpDirection);
        }
    }

    [SerializeField]
    private float _jumpHeight;

    [SerializeField]
    private float _jumpHorizontalDistanceMultiplier = 1f;

    [SerializeField]
    private float _jumpDuration = 1f;

    [SerializeField]
    private int _priority = 1;

    [SerializeField]
    [Tooltip("Must be even!")]
    protected int jumpFormulaExponent = 2;

    [SerializeField]
    private bool _endJumpAfterDuration;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private int _numberOfDrawingInGizmos;
    [SerializeField]
    private Vector3 _gizmosVelocity = Vector2.one * 5f;
    [SerializeField]
    [ReadOnly]
    private float _whereInCountIs;
    private JumpData _latestJump;
    [SerializeField]
    [ReadOnly]
    private string _latestJumpDebug;
#endif

#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
#endif
    private bool _isJumping;

    private float _jumpSpeedMultiplier = 1f;

    public event DelPlatformerConditionalEvent OnPlatformerGroundedStateChange;

    [SerializeField]
    private KinematicPlatformer _body;
    protected KinematicPlatformer Body
    {
        get
        {
            if (_body == null) _body = GetComponent<KinematicPlatformer>();
            return _body;
        }
    }

    private Coroutine routine;
    protected delegate Vector2 TimeToPositionFunction2D(float t);

    private WatchableState<bool> _ascending = new WatchableState<bool>();
    public WatchableState<bool> Ascending {
        get
        {
            return _ascending;
        }
    }

    

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = GizmosUtils.InformativeColor;

        JumpData gizmosData;
        if (!IsGrounded())
        {
            gizmosData = _latestJump;
        }
        else
        {
            gizmosData = new JumpData(transform, _gizmosVelocity);
        }

        for (int i = 0; i <= _numberOfDrawingInGizmos; i++)
        {
            float where = (float)i / _numberOfDrawingInGizmos;
            Vector3 pos = GetParabolaPositionOnTime(where, gizmosData, ParabolaFunction);
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
#endif

    public void Jump()
    {
        if (isActiveAndEnabled)
        {
            routine = StartCoroutine(FollowFuncRoutine(Body.AbsoluteVelocity * _jumpDuration, ParabolaFunction));
        }
    }

    public void SetJumpSpeedMultiplier(float multiplier)
    {
        _jumpSpeedMultiplier = multiplier;
    }

    public void SetJumpDuration(float duration)
    {
        SetJumpSpeedMultiplier(duration / GetCurrentJumpDuration());
    }

    public void UnsetJumpSpeed()
    {
        _jumpSpeedMultiplier = 1f;
    }

    public void InterruptJump()
    {
        StopAllCoroutines();
        EndJump();
    }

    public void Jump(Vector3 velocity)
    {
        if (isActiveAndEnabled)
        {
            StopAllCoroutines();
            routine = StartCoroutine(FollowFuncRoutine(velocity * _jumpDuration, ParabolaFunction));
        }
    }

    public void JumpForward(float force)
    {
        if (isActiveAndEnabled)
        {
            StopAllCoroutines();
            routine = StartCoroutine(FollowFuncRoutine(transform.forward * force * _jumpDuration, ParabolaFunction));
        }
    }

    private IEnumerator FollowFuncRoutine(Vector3 to, TimeToPositionFunction2D func)
    {
        Debug.LogFormat("[{0}] Started routine!", Time.frameCount);
        JumpData jump = StartJump(to);
        float distance = to.magnitude;
        //By getting everything into a parabola y = -Ax^2, and knowing that the distance is the distance between the roots, we arrive that A = 4h/Distance!
        //So we know the formula for y over x in the local space!
        float count;
        ResetTime(out count);
        Vector3 positionBefore = GetParabolaPositionOnTime(count, jump, func);
        Vector3 positionNow = positionBefore;
        do
        {
            AdvanceTime(ref count);
            positionBefore = positionNow;
            positionNow = GetParabolaPositionOnTime(count, jump, func);
            Advance(positionNow, positionBefore);
            yield return new WaitForFixedUpdate();
        } while (!IsGrounded() && !IsEndOfJump(count));
        
        EndJump();
        routine = null;
        Debug.LogFormat("[{0}] Ended routine!", Time.frameCount);
    }

    protected void ResetTime(out float timeCounter)
    {
        timeCounter = 0f;
        DebugSaveCount(timeCounter);
    }
    
    public float GetCurrentJumpDuration()
    {
        return _jumpDuration;
    }

    protected void AdvanceTime(ref float timeCounter)
    {
        timeCounter += (Time.fixedDeltaTime * _jumpSpeedMultiplier) / _jumpDuration;
        DebugSaveCount(timeCounter);
    }

    protected void Advance(Vector3 positionNow, Vector3 positionBefore)
    {
        Vector3 movement = positionNow - positionBefore;
        Ascending.Update(movement.y >= 0f);
        Body.AddExactNextFrameMove(positionNow - positionBefore, _priority);
    }

    protected bool IsEndOfJump(float timeCounter)
    {
        if (_endJumpAfterDuration)
            return timeCounter >= _jumpDuration;
        else return false;
    }
    
    public bool IsDoingJumpRoutine()
    {
        return _isJumping;
    }

    private void DebugSaveCount(float count)
    {
#if UNITY_EDITOR
        _whereInCountIs = count;
#endif
    }

    protected Vector3 GetParabolaPositionOnTime(float t, JumpData data, TimeToPositionFunction2D func)
    {
        return GetParabolaWorldPosition(func(t), data);
    }

    protected Vector2 ParabolaFunction(float t)
    {
        float dislocatedT = ((t * 2f) - 1f);
        return new Vector2(t, 1f - Mathf.Pow(dislocatedT, jumpFormulaExponent));
    }

    protected Vector2 HalfParabolaFunction(float t)
    {
        float dislocatedT = t * 2f;
        return new Vector2(t, 1f - Mathf.Pow(dislocatedT, jumpFormulaExponent));
    }   

    private Vector3 GetParabolaWorldPosition(Vector2 localParabola, JumpData data)
    {
        float horizontalMultiplier = localParabola.x;
        float verticalMultiplier = localParabola.y;
        Vector3 pos3DParabolaLocalPos = new Vector3(
            data.jumpDirection.x * horizontalMultiplier,
            _jumpHeight * verticalMultiplier,
            data.jumpDirection.z * horizontalMultiplier
        );
        Vector3 pos3DParabolaWorldPos = data.origin + pos3DParabolaLocalPos;
        return pos3DParabolaWorldPos;
    }

    protected JumpData GetJumpDataWithThisStartingPosition(Vector3 vel)
    {
        return new JumpData(transform, vel * _jumpHorizontalDistanceMultiplier);
    }

    protected virtual JumpData StartJump(Vector3 vel)
    {
        _isJumping = true;
        JumpData data = GetJumpDataWithThisStartingPosition(vel);
#if UNITY_EDITOR
        _latestJump = data;
        _latestJumpDebug = data.ToString();
#endif
        return data;
    }

    protected virtual void EndJump()
    {
        _isJumping = false;
    }

    protected bool IsGrounded()
    {
        return Body.Grounded;
    }
}
