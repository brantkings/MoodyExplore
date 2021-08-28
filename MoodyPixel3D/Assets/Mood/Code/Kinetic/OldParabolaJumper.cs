using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using LHH.Caster;
using LHH.Utils;

public class OldParabolaJumper : CustomPhysicsController, IPlatformer
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
            jumpDirection = direction;
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
    private bool _endJumpAfterDuration;

    [SerializeField]
    private Caster _wallCaster;

    [SerializeField]
    private Caster _groundCaster;

    [SerializeField]
    private Transform _feetPosition;

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

    private bool _isJumping;
    private Vector3 _lastGroundedPosition;

    public bool NeedExternalCheck => false;

    public event DelPlatformerConditionalEvent OnPlatformerGroundedStateChange;

    public GroundedState IsGrounded()
    {
        if (_isJumping)
        {
            return GroundedState.Aerial;
        }
        else return GroundedState.Neither;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = GizmosUtils.InformativeColor;

        JumpData gizmosData;
        if (IsJumping())
        {
            gizmosData = _latestJump;
        }
        else
        {
            gizmosData = new JumpData(transform, _gizmosVelocity);
        }

        for(int i = 0;i<=_numberOfDrawingInGizmos;i++)
        {
            float where = (float)i / _numberOfDrawingInGizmos;
            Vector3 pos = GetParabolaPositionOnTime(where, gizmosData);
            Gizmos.DrawSphere(pos, 0.1f);
        }
    }
#endif

    public void Jump()
    {
        if(isActiveAndEnabled)
        {
            StartCoroutine(JumpRoutine(Body.Velocity * _jumpDuration));
        }
    }

    public void JumpForward(float force)
    {
        if (isActiveAndEnabled)
        {
            StartCoroutine(JumpRoutine(transform.forward * force * _jumpDuration));
        }
    }

    private IEnumerator JumpRoutine(Vector3 to)
    {
        JumpData jump = StartJump(to);
        float distance = to.magnitude;
        //By getting everything into a parabola y = -Ax^2, and knowing that the distance is the distance between the roots, we arrive that A = 4h/Distance!
        //So we know the formula for y over x in the local space!

        float count = 0f;
        DebugSaveCount(count);
        Vector3 positionBefore = GetParabolaPositionOnTime(count, jump);
        Vector3 positionNow = positionBefore;
        RaycastHit groundHit = new RaycastHit();
        do
        {
            count += Time.fixedDeltaTime / _jumpDuration;
            DebugSaveCount(count);
            //Body.AddForce(JUMPER_ID, (positionNow - positionBefore) * (1f / Time.fixedDeltaTime)); //This is very hard to do because the force is calculated in the body per FixedUpdate again!
            positionBefore = positionNow;
            positionNow = GetParabolaPositionOnTime(count, jump);
            AdvanceWhileLookingForWall(positionNow, positionBefore, ref jump);
            //Body.SetPosition(positionNow);
            yield return new WaitForFixedUpdate();
        } while ((IsAscending(positionNow, positionBefore) || !CheckForGround(out groundHit)) && IsJumpValidByTime(count));

        positionBefore = positionNow;

        if(groundHit.collider != null)
        {
            positionNow.y = GetGroundedPosition(groundHit.point).y;
        }
        AdvanceWhileLookingForWall(positionNow, positionBefore, ref jump);
        //Body.SetPosition(positionNow);

        EndJump();
    }

    private void AdvanceWhileLookingForWall(Vector3 positionNow, Vector3 positionBefore, ref JumpData jump)
    {
        RaycastHit wallHit;
        if (CheckForWall(positionNow + Vector3.up, positionNow - positionBefore, out wallHit))
        {
            ChangeJumpToAccountFromWall(wallHit, ref jump);
        }
        else
        {
            Body.SetPosition(positionNow);
        }
    }

    private bool IsAscending(Vector3 pNow, Vector3 pBefore)
    {
        return pNow.y - pBefore.y > 0;
    }

    private bool IsJumpValidByTime(float count)
    {
        if (_endJumpAfterDuration)
            return count <= 1f;
        else return true;
    }

    private void DebugSaveCount(float count)
    {
#if UNITY_EDITOR
        _whereInCountIs = count;
#endif
    }

    private bool CheckForWall(Vector3 origin, Vector3 velocity, out RaycastHit hit)
    {
        if (_wallCaster != null)
        {
            if (_wallCaster.CastLength(origin, velocity, out hit))
            {
                return true;
            }
            return false;
        }
        else
        {
            hit = new RaycastHit();
            return false;
        }
    }

    private bool CheckForGround(out RaycastHit hit)
    {
        hit = new RaycastHit();
        if (_groundCaster == null) return false;
        else
        {
            if (_groundCaster.Cast(out hit))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    

    private Vector3 GetGroundedPosition(Vector3 point)
    {
        return point + (_feetPosition.position - transform.position);
    }

    private void ChangeJumpToAccountFromWall(RaycastHit hit, ref JumpData data)
    {
        /*data.jumpDirection = Vector3.ProjectOnPlane(data.jumpDirection, hit.normal);
        Vector3 backToOrigin = data.origin - Body.GetPosition();
        Vector3 newOrigin = Body.GetPosition() - data.jumpDirection.normalized * backToOrigin.magnitude;
        newOrigin.y = data.origin.y;
        data.origin = newOrigin;*/

        Vector3 hitPosition = _wallCaster.GetCenterPositionOfHit(hit);
        data.origin.x = hitPosition.x;
        data.origin.z = hitPosition.z;
        data.jumpDistance = 0f;
        data.jumpDirection = Vector3.zero;
    }


    public Vector3 GetParabolaPositionOnTime(float t, JumpData data)
    {
        return GetParabolaWorldPosition(GetPositionInParabola(t), data);
    }

    private Vector2 GetPositionInParabola(float t)
    {
        float dislocatedT = ((t*2f) - 1f);
        return new Vector2(t, 1f - Mathf.Pow(dislocatedT, 2));
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
    
    protected virtual JumpData StartJump(Vector3 vel)
    {
        Body.LockGravity(JUMPER_ID);
        _isJumping = true;
        JumpData data = new JumpData(transform, vel);
        OnPlatformerGroundedStateChange?.Invoke(this, IsGrounded());
#if UNITY_EDITOR
        _latestJump = data;
        _latestJumpDebug = data.ToString();
#endif
        return data;
    }

    protected virtual void EndJump()
    {
        _lastGroundedPosition = transform.position;
        _isJumping = false;
        OnPlatformerGroundedStateChange?.Invoke(this, IsGrounded());
        Body.SetForce(JUMPER_ID, Vector3.zero);
        Body.UnlockGravity(JUMPER_ID);
    }

    private bool IsJumping()
    {
        return _isJumping;
    }

    public Vector3 GetGroundedPositionOfLatestCheck()
    {
        return _lastGroundedPosition;
    }

    public void CheckPlatform()
    {
        //Dont do anything
    }
}
