using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LHH.Caster;

public class CustomPhysicsBody : RigidbodyController, IHorizontalMover
{
    public static string PLATFORMER_LOCK_GRAVITY_ID = "OwnPlatformer";

    private Vector3 _currentPos;

    public bool controlKinematic = true;
    public Platformer groundedPlatformer;
    public Caster wallCaster;
    public float maxFallVelocity;
    public float timeToMaxVelocity;
    private HashSet<string> _gravityLockers;

    [SerializeField]
    [ReadOnly]
    private float _fallingVelocityNow;
    private float _fallingVelocityDelta;

    private bool _originallyKinematic;

    private struct ExternalForcesStructure
    {
        private Dictionary<string, Vector3> forces;

        public Vector3 this[string index]
        {
            get
            {
                Vector3 val = Vector3.zero;
                forces.TryGetValue(index, out val);
                return val;
            }
            set
            {
                if (!forces.ContainsKey(index)) forces.Add(index, value);
                else forces[index] = value;
            }
        }

        public ExternalForcesStructure(int capacity)
        {
            forces = new Dictionary<string, Vector3>(capacity);
        }

        public IEnumerable<Vector3> Values
        {
            get
            {
                return forces.Values;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return forces.Keys;
            }
        }
    }

    private ExternalForcesStructure _externalForces;
    [Space]
    [SerializeField]
    [ReadOnly]
    private Vector3 _externalForcesDebug;
    public float horizontalDrag;
    
    public float FallVelocity
    {
        get
        {
            if (IsGravityLocked())
                return 0f;
            else
                return _fallingVelocityNow;
        }
    }
    
    public Vector3 GravityForce
    {
        get
        {
            return FallVelocity * -Vector3.up;
        }
    }

    public Vector3 ExternalForces
    {
        get
        {
            Vector3 vec = Vector3.zero;
            foreach(var value in _externalForces.Values) vec += value;
#if UNITY_EDITOR
            _externalForcesDebug = vec;
#endif
            return vec;
        }
    }

    public Vector3 HorizontalVelocity
    {
        get
        {
            Vector3 forces = ExternalForces;
            forces.y = 0f;
            return forces;
        }
    }
    
    public Vector3 Velocity
    {
        get
        {
            return GravityForce + ExternalForces;
        }
    }

    protected Vector3 VelocityByFrame
    {
        get
        {
            return Velocity * Time.fixedDeltaTime;
        }
    }


    private void Awake()
    {
        _gravityLockers = new HashSet<string>();
        _externalForces = new ExternalForcesStructure(5);
        _originallyKinematic = Body.isKinematic;
        _currentPos = Body.position;
    }

    private void OnEnable()
    {
        if(groundedPlatformer != null) groundedPlatformer.Interface.OnPlatformerGroundedStateChange += OnGroundedStateChange;
        if (controlKinematic) Body.isKinematic = true;
    }


    private void OnDisable()
    {
        if (groundedPlatformer != null) groundedPlatformer.Interface.OnPlatformerGroundedStateChange -= OnGroundedStateChange;
        if (controlKinematic)
        {
            Body.isKinematic = _originallyKinematic;
            Body.velocity = Vector3.zero;
        }
    }


    private void FixedUpdate()
    {
        //TODO make it calculate external forces only once per fixed frame
        bool? needs = groundedPlatformer?.Interface?.NeedExternalCheck;
        if (needs.HasValue && needs.Value) groundedPlatformer?.Interface?.CheckPlatform();
        
        if (!IsGravityLocked()) GravityFixedUpdate();

        ApplyHorizontalDrag();

        Vector3 velocity = Velocity;
        Vector3 velocityThisFrame = velocity * Time.fixedDeltaTime;

        CheckWalls(ref _currentPos, ref velocityThisFrame);
        
        MoveByVelocityFixedUpdate(velocityThisFrame, ref _currentPos);
        Body.MovePosition(_currentPos);
    }

    private void OnGroundedStateChange(IPlatformer plat, GroundedState state)
    {
        switch (state)
        {
            case GroundedState.Neither:
                break;
            case GroundedState.Grounded:
                CancelFall();
                if (groundedPlatformer.Interface != null)
                    _currentPos.y = groundedPlatformer.Interface.GetGroundedPositionOfLatestCheck().y;
                break;
            case GroundedState.Aerial:
                break;
            default:
                break;
        }
    }

    public void LockGravity(string id)
    {
        _gravityLockers.Add(id);
        CancelFall();
    }

    public void UnlockGravity(string id)
    {
        _gravityLockers.Remove(id);
    }

    public bool IsGravityLocked()
    {
        return _gravityLockers.Count > 0 || IsGrounded();
    }

    public bool IsGrounded()
    {
        if (groundedPlatformer != null && groundedPlatformer.Interface != null) return groundedPlatformer.Interface.IsGrounded() == GroundedState.Grounded;
        else return false;
    }

    private void GravityFixedUpdate()
    {
        _fallingVelocityNow = Mathf.SmoothDamp(_fallingVelocityNow, maxFallVelocity, ref _fallingVelocityDelta, timeToMaxVelocity, maxFallVelocity, Time.deltaTime);
    }

    private void CancelFall()
    {
        _fallingVelocityNow = _fallingVelocityDelta = 0f;
    }

    private bool CheckWalls(ref Vector3 pos, ref Vector3 vel)
    {
        bool checkedWall = false;
        if (wallCaster != null && vel.sqrMagnitude > 0f)
        {
            while(wallCaster.CastLength(vel, vel.magnitude, out RaycastHit hit))
            {
                //pos = wallCaster.GetCasterCenterOfHit(hit);
                vel = Vector3.ProjectOnPlane(vel, hit.normal);
                checkedWall = true;
            }
        }
        return checkedWall;
    }

    private void ApplyHorizontalDrag()
    {
        foreach(var key in _externalForces.Keys.ToList())
        {
            float dragMagnitude = horizontalDrag; //TODO maybe a better
            var value = _externalForces[key];
            if (value.magnitude < dragMagnitude)
            {
                _externalForces[key] = Vector3.zero;
            }
            {
                Vector3 groundVel = MakeHorizontalVector(value);
                value -= groundVel.normalized * dragMagnitude;
                if (value.sqrMagnitude < 0.1f) value = Vector3.zero;

                _externalForces[key] = value;
            }
        }
    }

    protected Vector3 MakeHorizontalVector(Vector2 vec)
    {
        return new Vector3(vec.x, 0f, vec.y);
    }
    protected Vector3 MakeHorizontalVector(Vector2 vec, Vector3 keepY)
    {
        return new Vector3(vec.x, keepY.y, vec.y);
    }

    protected Vector3 MakeHorizontalVector(Vector3 vec)
    {
        return new Vector3(vec.x, 0f, vec.z);
    }

    protected Vector3 MakeHorizontalVector(Vector3 vec, Vector3 keepY)
    {
        return new Vector3(vec.x, keepY.y, vec.z);
    }

    private void MoveByVelocityFixedUpdate(Vector3 velocityThisFrame, ref Vector3 pos)
    {
        pos = pos + velocityThisFrame;
    }

    #region OutsideInterface
    public void AddForce(string id, Vector3 force)
    {
        _externalForces[id] += force;
    }

    public void AddHorizontalForce(string id, Vector3 force)
    {
        _externalForces[id] += MakeHorizontalVector(force);
    }

    public void AddHorizontalForce(string id, Vector2 force)
    {
        _externalForces[id] += MakeHorizontalVector(force);
    }

    public void SetForce(string id, Vector3 force)
    {
        _externalForces[id] = force;
    }

    public void SetHorizontalForce(string id, Vector3 force)
    {
        _externalForces[id] = MakeHorizontalVector(force, _externalForces[id]);
    }

    public void SetHorizontalForce(string id, Vector2 force)
    {
        _externalForces[id] = MakeHorizontalVector(force, _externalForces[id]);
    }

    public void SetPosition(Vector3 pos)
    {
        _currentPos = pos;
    }

    public Vector3 GetPosition()
    {
        return _currentPos;
    }


    public void RemoveAllForces()
    {
        foreach(var key in _externalForces.Keys)
        {
            _externalForces[key] = Vector3.zero;
        }
    }
    #endregion

    public void Move(Vector2 move)
    {
        SetHorizontalForce("MOVER_INTERFACE", move);
    }
    
    [ContextMenu("Debug external forces")]
    public void DebugExternalForces()
    {
        foreach (var key in _externalForces.Keys)
        {
            Debug.LogFormat("{0} is {1}", key, _externalForces[key]);
        }
    }
}
