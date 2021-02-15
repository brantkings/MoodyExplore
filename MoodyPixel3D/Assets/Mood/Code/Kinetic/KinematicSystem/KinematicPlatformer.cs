using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;

public interface IKinematicPlatformerVelocityGetter
{
    Vector3 GetVelocity();
}
public interface IKinematicPlatformerFrameVelocityGetter
{
    Vector3 GetFrameVelocity(float deltaTime);
}

public class KinematicPlatformer : MonoBehaviour
{
    public enum CasterClass
    {
        Ground,
        Ceiling,
        Side,
        None
    }

    public static float GRAVITY = 10f;

    public Caster groundCaster;
    public Caster ceilingCaster;
    public Caster wallCaster;
    [SerializeField]
    private bool _setPositionDirectlyToTransform;

    public bool _hasGravity;
    public bool preciseWallCorrections = true;

    private Rigidbody _body;

    public float mass = 1f;
    public float airResistanceCoefficient = 0f;
    public float minVelocityMagnitude = 0.1f;
    
#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
#endif
    private Vector3 _velocity;

#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
#endif
    private Vector3 _torque;
#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
#endif
    private Vector3 _currentInputVelocity;
#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
    private Vector3 _currentOtherSourcesInputVelocity;
#endif
    private Vector3 _setFrameMovement;
    private LinkedList<IKinematicPlatformerVelocityGetter> _velocitySources;
    private LinkedList<IKinematicPlatformerFrameVelocityGetter> _frameVelocitySources;

    private Vector3 _latestValidVelocity;

    private HashSet<string> gravityLock;

    private WatchableState<bool> _grounded;
    public WatchableState<bool> Grounded { get { if (_grounded == null) _grounded = new WatchableState<bool>(); return _grounded; } }

    private WatchableState<bool> _walled;
    public WatchableState<bool> Walled { get { if (_walled == null) _walled = new WatchableState<bool>(); return _walled; } }

    private WatchableState<bool> _hittingHead;
    public WatchableState<bool> HittingHead { get { if (_hittingHead == null) _hittingHead = new WatchableState<bool>(); return _hittingHead; } }

#if UNITY_EDITOR
    [ReadOnly]
    [SerializeField]
    private bool _debugGrounded;
    [ReadOnly]
    [SerializeField]
    private int _debugWalled;
    [ReadOnly]
    [SerializeField]
    private bool _debugHittingHead;
    [ReadOnly]
    [SerializeField]
    private Vector3 _lastTotalVelDebug;
    [ReadOnly]
    [SerializeField]
    private Vector3 _lastValidVelDebug;

#endif
    
    [SerializeField]
    private bool _debug;

    private bool Debugging
    {
        get
        {
#if UNITY_EDITOR
            return _debug;
#else
            return false;
#endif
        }
    }

    private LinkedList<IKinematicPlatformerVelocityGetter> Sources
    {
        get
        {
            if (_velocitySources == null) _velocitySources = new LinkedList<IKinematicPlatformerVelocityGetter>();
            return _velocitySources;
        }
    }

    private LinkedList<IKinematicPlatformerFrameVelocityGetter> FrameSources
    {
        get
        {
            if (_velocitySources == null) _frameVelocitySources = new LinkedList<IKinematicPlatformerFrameVelocityGetter>();
            return _frameVelocitySources;
        }
    }

    private void Awake()
    {
        _body = GetComponentInParent<Rigidbody>();
    }

    public void AddForce(Vector3 force)
    {
        if(mass != 0f)
        {
            _velocity += force / mass;
        }
    }

    public void RemoveAllForces()
    {
        _velocity = Vector3.zero;
    }

    
    public Vector3 Position
    {
        protected set
        {
            if (!_setPositionDirectlyToTransform && _body != null)
            {
                _body.position = value;
            }
            else
            {
                transform.position = value;
            }
        } 
        get
        {
            if (!_setPositionDirectlyToTransform && _body != null)
            {
                return _body.position;
            }
            else
            {
                return transform.position;
            }
        }
    }
    public void SetVelocity(Vector3 vel)
    {
        //if (vel != _currentInputVelocity) Debug.LogFormat("Setting Velocity of {0} as {1}", transform.root, vel.ToString("F3"));
        _currentInputVelocity = vel;
    }

    public void AddVelocitySource(IKinematicPlatformerVelocityGetter getter)
    {
        Sources.AddLast(getter);
    }

    public void RemoveVelocitySource(IKinematicPlatformerVelocityGetter getter)
    {
        Sources.Remove(getter);
    }

    public void AddVelocitySource(IKinematicPlatformerFrameVelocityGetter getter)
    {
        FrameSources.AddLast(getter);
        Debug.LogFormat("Added {0} and has {1}", getter, Sources.Count);
    }

    public void RemoveVelocitySource(IKinematicPlatformerFrameVelocityGetter getter)
    {
        FrameSources.Remove(getter);
        Debug.LogFormat("Removed {0} and has {1}", getter, Sources.Count);
    }

    public void AddVelocity(Vector3 vel)
    {
        _currentInputVelocity += vel;
    }

    public void CancelVelocity()
    {
        _currentInputVelocity = Vector3.zero;
    }

    public void AddExactNextFrameMove(Vector3 move)
    {
        _setFrameMovement += move;
    }

    private Vector3 ExtractNextFrameMove(float deltaTime)
    {
        Vector3 temp = _setFrameMovement;
        _setFrameMovement = Vector3.zero;
        foreach (IKinematicPlatformerFrameVelocityGetter getter in FrameSources)
        {
            if(!getter.Equals(null))
            {
                Vector3 velFrame = getter.GetFrameVelocity(deltaTime);
                if (velFrame != Vector3.zero)
                {
                    temp += velFrame;
                }
            }
        }
        return temp;
    }

    protected bool IsGrounded()
    {
        return Grounded;
    }

    protected bool HasGravity()
    {
        return _hasGravity && (gravityLock == null || gravityLock.Count <= 0);
    }

    public void LockGravity(string id)
    {
        if (gravityLock == null) gravityLock = new HashSet<string>();
        gravityLock.Add(id);
        _grounded.Update(false);
    }

    public void UnlockGravity(string id)
    {
        if (gravityLock != null)
        {
            gravityLock.Remove(id);
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return _latestValidVelocity / Time.fixedDeltaTime;
        }
    }

    private void AffectNaturalVelocityForcesCausedByVelocity(ref Vector3 vel)
    {
        vel -= (vel.sqrMagnitude * vel.normalized) * airResistanceCoefficient * Time.fixedDeltaTime; //Resistance = v^2 * K
        if (vel.sqrMagnitude < (minVelocityMagnitude * minVelocityMagnitude)) vel = Vector3.zero;
    }

    private Vector3 GetNaturalVelocity()
    {
        AffectNaturalVelocityForcesCausedByVelocity(ref _velocity);
        return _velocity;
    }

    private Vector3 GetCurrentVelocity()
    {
        Vector3 sourceVel = GetNaturalVelocity();
        if(_velocitySources != null)
        {
            foreach(IKinematicPlatformerVelocityGetter getter in _velocitySources)
            {
                sourceVel += getter.GetVelocity();
            }
        }
#if UNITY_EDITOR
        _currentOtherSourcesInputVelocity = sourceVel;
#endif
        return _currentInputVelocity + sourceVel;
    }


    private bool debugWasDoingIt;

    public static Vector3 GetGravityForce()
    {
        return Vector3.down * GRAVITY;
    }

    void FixedUpdate()
    {
        Vector3 totalMovement = GetCurrentVelocity();
        
        if (HasGravity()) totalMovement += GetGravityForce();

        Vector3 extractMovement = ExtractNextFrameMove(Time.fixedDeltaTime);
        Vector3 frameMovement = totalMovement * Time.fixedDeltaTime + extractMovement;
#if UNITY_EDITOR
        _lastTotalVelDebug = frameMovement / Time.fixedDeltaTime;
#endif
        Vector3 horizontalMovement = GetHorizontalMovement(frameMovement);
        Vector3 verticalMovement = GetVerticalMovement(frameMovement);

        Vector3 movementMade = Vector3.zero;

        bool nowGrounded = Grounded, nowHittingHead = HittingHead, nowWalled = Walled;
        //Horizontal movement resolve
        CheckAndReflectMovement(ref horizontalMovement, out Vector3 lateralReflection, out nowWalled, movementMade, wallCaster, preciseWallCorrections);
        
        //Divide vertical movement and add total horizontal movement into final movement.
        verticalMovement += Vector3.Project(horizontalMovement, Vector3.up);
        movementMade += Vector3.ProjectOnPlane(horizontalMovement, Vector3.up);

        //After moving horizontally, then checks the ground so players can squeeze better through places.
        Vector3 verticalReflection;
        if (verticalMovement.y > 0f) CheckAndStopMovement(ref verticalMovement, out verticalReflection, out nowHittingHead, movementMade, ceilingCaster);
        else CheckAndStopMovement(ref verticalMovement, out verticalReflection, out nowGrounded, movementMade, groundCaster);

        movementMade += verticalMovement;

        //Reflection and friction forces resolve
        Vector3 totalReflection = lateralReflection + verticalReflection;
        /*if(nowGrounded && !Grounded) //Is going to be grounded
        {

        }
        if(nowWalled && !Walled) //Is going to be walled
        {

        }*/

        _grounded.Update(nowGrounded, "Grounded");
        _hittingHead.Update(nowHittingHead, "HittingHead");
        _walled.Update(nowWalled, "Walled");
        

        Move(movementMade);

#if UNITY_EDITOR
        _debugGrounded = Grounded;
        _debugHittingHead = HittingHead;
        _debugWalled = Walled ? 1 : 0;
#endif
    }

    private Vector3 GetHorizontalMovement(Vector3 totalMovement)
    {
        return new Vector3(totalMovement.x, 0f, totalMovement.z);
    }

    private Vector3 GetVerticalMovement(Vector3 totalMovement)
    {
        return new Vector3(0f, totalMovement.y, 0f);
    }

    private Vector3 ApproachColliderThroughNormal(Caster caster, RaycastHit hit, Vector3 originOffset, Vector3 movementToHitWall)
    {
        Vector3 lateralAmountToHugWall = Vector3.Project(movementToHitWall, hit.normal); //This didn't let corners have different resting positions for difference forces.

        if (caster.CastLengthOffset(originOffset, lateralAmountToHugWall, out RaycastHit precisionHit))
        {
            return lateralAmountToHugWall.normalized * precisionHit.distance;
        }
        else
        {
            return lateralAmountToHugWall;
        }
    }

    private void CheckAndReflectMovement(ref Vector3 movement, out Vector3 reflection, out bool foundCast, Vector3 originOffset, Caster caster, bool hugColliderApproachingNormal)
    {
        foundCast = false;
        reflection = Vector3.zero;
        if (caster != null)
        {
            Vector3 oldMove = movement;
            //Debug.LogFormat("[{0}] Checking {1} with offset {2} ({3}) ", Time.frameCount, movement.ToString("F3"), originOffset.ToString("F3"), caster);

            Vector3 movementToGlueWithWall = Vector3.zero;
            int numberOfHits = 0;
            while (movement != Vector3.zero && caster.CastLengthOffset(originOffset, movement, out RaycastHit hit))
            {
                float moveMagnitude = movement.magnitude;
                if(hit.distance < moveMagnitude) foundCast = true; //If movement is going to end up inside the collider, it is a solid hit

                Vector3 amountToHugWall = movement.normalized * Mathf.Min(hit.distance, moveMagnitude); //Hug the wall.
                Vector3 toGlueWithWallThisTime;
                if (hugColliderApproachingNormal)
                    toGlueWithWallThisTime = ApproachColliderThroughNormal(caster, hit, originOffset, amountToHugWall); //With this on, at least two casts will be done
                else
                    toGlueWithWallThisTime = amountToHugWall;

                movementToGlueWithWall += toGlueWithWallThisTime;
                reflection += Vector3.Project(movement - toGlueWithWallThisTime, hit.normal);

                movement = Vector3.ProjectOnPlane(movement, hit.normal);

                if (++numberOfHits > 10)
                {
                    Debug.LogError("Infinite loop!");
                    break;
                }
            }

            movement += movementToGlueWithWall;
            
            
        }
    }

    public Collider WhatIsWhereIAmTryingToGo(CasterClass caster, out RaycastHit hit)
    {
        return WhatIsInThere(_currentInputVelocity + _currentOtherSourcesInputVelocity, caster, out hit);
    }

    public Collider WhatIsInThere(Vector3 checkDistance, CasterClass caster, out RaycastHit hit)
    {
        return WhatIsInThere(checkDistance, GetCaster(caster), out hit);
    }

    public Caster GetCaster(CasterClass caster)
    {
        switch (caster)
        {
            case CasterClass.Ground:
                return groundCaster;
            case CasterClass.Ceiling:
                return ceilingCaster;
            case CasterClass.Side:
                return wallCaster;
            default:
                return null;
        }
    }

    private Collider WhatIsInThere(Vector3 checkDistance, Caster caster, out RaycastHit hit)
    {
        hit = default(RaycastHit);
        if (caster != null && caster.Cast(checkDistance, out hit))
        {
            return hit.collider;
        }
        else return null;
    }

    private void CheckAndStopMovement(ref Vector3 movement, out Vector3 reflection, out bool foundCast, Vector3 originOffset, Caster caster)
    {
        foundCast = false;
        reflection = Vector3.zero;
        if (caster != null)
        {
            Vector3 oldMove = movement;
            if (movement != Vector3.zero && caster.CastLengthOffset(originOffset, movement, out RaycastHit hit))
            {
                float moveMagnitude = movement.magnitude;
                if (hit.distance < moveMagnitude)
                {
                    foundCast = true; //If movement is going to end up inside the collider, it is a solid hit
                    Vector3 amountToHugWall = movement.normalized * hit.distance;
                    reflection += Vector3.ProjectOnPlane(movement - amountToHugWall, hit.normal);
                    movement = amountToHugWall;
                }
            }
        }
    }

    private void SolveReflection(Vector3 inVel, float minVelToReflect, float reflectionAbsortion)
    {

    }


    private void Move(Vector3 movement)
    {
        _latestValidVelocity = movement;
#if UNITY_EDITOR
        _lastValidVelDebug = _latestValidVelocity / Time.fixedDeltaTime;
#endif
        if (movement != Vector3.zero)
        {
            if (Debugging)
                Debug.LogFormat("{0} going to {1} + {2} = {3}", 
                    this, Position, movement.ToString("F4"), Position + movement);
            Position = Position + movement;
        }
    }
}



