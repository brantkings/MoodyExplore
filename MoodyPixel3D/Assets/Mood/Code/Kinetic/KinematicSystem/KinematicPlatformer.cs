using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;
using System.Linq;

public interface IKinematicPlatformerVelocityGetter
{
    Vector3 GetVelocity();
}

public interface IKinematicPlatformerFrameVelocityGetter
{
    Vector3 GetFrameVelocity(float deltaTime);
}

public interface IKinematicPlatformerVelocityGetterActivateable
{
    void StartVelocity();
}

public interface IKinematicPlatformerVelocityGetterInputModifier
{
    bool IsInputVelocity();
}

public partial class KinematicPlatformer : MonoBehaviour
{
    public enum CasterClass
    {
        Ground,
        Ceiling,
        Side,
        None
    }

    public static float SMALL_AMOUNT = 0.01f;
    public static float SMALL_AMOUNT_SQRD = SMALL_AMOUNT * SMALL_AMOUNT;
    public static float GRAVITY = 10f;

    public Caster groundCaster;
    public Caster ceilingCaster;
    public Caster wallCaster;

    private Rigidbody _body;
    public enum ManipulatingStyle
    {
        Transform,
        RigidbodyInParent,
        RigidbodyInChild,
    }
    public ManipulatingStyle manipulatingStyle = ManipulatingStyle.RigidbodyInParent;
    
    [System.Serializable]
    private class MovementData
    {
        public float airResistanceCoefficient = 0f;
        public float minMagnitude = 0.1f;
        public float reflectionMultiplier = 0f;
        public float dragMultiplier = 1f;
    }

    public float airResistanceCoefficient = 0f;
    public float minVelocityMagnitude = 0.1f;
    public float reflectionMultiplier = 0f;
    public float dragMultiplier = 1f;

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
    private Vector3 _debugCurrentOtherSourcesInputVelocity;
#endif
    private Dictionary<int,Vector3> _setFrameMovement;


    private Vector3 _outsideVelocity;
    private Vector3 _latestNonZeroValidMovement;
    private Vector3 _latestValidVelocity;

    private Vector3 _desiredDirection;

    private Transform root;

    private struct PlatformState
    {
        internal Collider platform;
        internal Rigidbody platformBody;
        internal Vector3 untransformedContactPoint;
        internal Quaternion currentRotation;
    }

    private PlatformState _currentPlatform;

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
    private bool _debugWalled;
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

#if UNITY_EDITOR
    internal struct CastHistoryElement
    {
        internal Vector3 offset;
        internal Vector3 distance;
        internal Caster caster;
        internal RaycastHit hit;
        internal string comment;
    }

    internal class CastHistory : IEnumerable<CastHistoryElement>
    {
        public List<CastHistoryElement> elements;

        public void Add(CastHistoryElement elem)
        {
            elements.Add(elem);
        }

        public CastHistory()
        {
            elements = new List<CastHistoryElement>(8);
        }

        public IEnumerator<CastHistoryElement> GetEnumerator()
        {
            return ((IEnumerable<CastHistoryElement>)elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)elements).GetEnumerator();
        }
    }

    private CastHistory history;
#endif

    private class VelocityGetter<T>
    {
        public Dictionary<int, LinkedList<T>> _values;

        public T Current => throw new System.NotImplementedException();

        private int _latestPriority;

        public VelocityGetter()
        {
            _values = new Dictionary<int, LinkedList<T>>(8);
        }

        public int BiggestPriority
        {
            get
            {
                if (_values == null || _values.Count <= 0) return int.MinValue;
                {
                    return _values.Max(GetKey);
                }
            }
        }

        private int GetKey(KeyValuePair<int, LinkedList<T>> x)
        {
            return x.Key;
        }


        public LinkedList<T> this[int priority]
        {
            get
            {
                if (!_values.ContainsKey(priority))
                {
                    _values.Add(priority, new LinkedList<T>());
                }
                return _values[priority];
            }
        }

        private KeyValuePair<int, LinkedList<T>> FindMaxValue(KeyValuePair<int, LinkedList<T>> x, KeyValuePair<int, LinkedList<T>> y)
        {
            return x.Key > y.Key ? x : y;
        }

        public LinkedList<T> GetBiggestPriorityList(out int biggestPriority)
        {
            biggestPriority = int.MinValue;
            _latestPriority = biggestPriority;
            LinkedList<T> t = null;
            if (_values.Count > 0)
            {
                foreach (KeyValuePair<int, LinkedList<T>> v in _values)
                {
                    if (v.Key > biggestPriority)
                    {
                        t = v.Value;
                        biggestPriority = v.Key;
                        _latestPriority = biggestPriority;
                    }
                }
            }
            return t;
        }

        public int GetLatestPriority()
        {
            return _latestPriority;
        }
    }

    private VelocityGetter<IKinematicPlatformerVelocityGetter> _velocitySources;
    private VelocityGetter<IKinematicPlatformerFrameVelocityGetter> _frameVelocitySources;

    private VelocityGetter<IKinematicPlatformerVelocityGetter> Sources
    {
        get
        {
            if (_velocitySources == null) _velocitySources = new VelocityGetter<IKinematicPlatformerVelocityGetter>();
            return _velocitySources;
        }
    }

    private VelocityGetter<IKinematicPlatformerFrameVelocityGetter> FrameSources
    {
        get
        {
            if (_frameVelocitySources == null) _frameVelocitySources = new VelocityGetter<IKinematicPlatformerFrameVelocityGetter>();
            return _frameVelocitySources;
        }
    }

    private void Awake()
    {
        switch (manipulatingStyle)
        {
            case ManipulatingStyle.RigidbodyInChild:
                _body = GetComponentInChildren<Rigidbody>();
                break;
            default:
                _body = GetComponentInParent<Rigidbody>();
                break;
        }

        root = transform.root;
        if (_desiredDirection == Vector3.zero) _desiredDirection = ActualDirection;
    }

    public void AddForce(Vector3 force)
    {
        float m = Mass;
        if (m != 0f)
        {
            _velocity += force / m;
        }
    }

    public void RemoveAllForces()
    {
        _velocity = Vector3.zero;
    }

    private int _frame;
    private int count;

    public Vector3 Position
    {
        protected set
        {
#if UNITY_EDITOR
            if(Debugging)
            {
                if(_frame != Time.frameCount)
                {
                    count = 0;
                    _frame = Time.frameCount;
                }
                count++;
                //Debug.LogFormat("Going from [{0} to {1}] for the {2}th time in frame {3}", _body.position.ToString("F4"), value.ToString("F4"), count, Time.frameCount);
                //if (count > 1) return;
            }
#endif

            if (UsingRigidbody())
            {
                //_body.position = value;
                _body.MovePosition(value);
            }
            else
            {
                transform.position = value;
            }
        }
        get
        {
            if (UsingRigidbody())
            {
                return _body.position;
            }
            else
            {
                return transform.position;
            }
        }
    }

    public Vector3 Direction
    {
        get => _desiredDirection;
        set
        {
            _desiredDirection = value;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            return Quaternion.LookRotation(_desiredDirection);
        }
        set
        {
            _desiredDirection = value * Vector3.forward;
        }
    }

    public Vector3 ActualDirection
    {
        get
        {
            if (UsingRigidbody())
            {
                return _body.rotation * Vector3.forward;

            }
            else
            {
                return transform.forward;
            }

        }
    }

    public Quaternion ActualRotation
    {
        get
        {
            if (UsingRigidbody())
            {
                return _body.rotation;

            }
            else
            {
                return transform.rotation;
            }
        }
    }

    private void SetActualDirection(Vector3 value)
    {
        if (value.sqrMagnitude != 0f)
        {
            if (UsingRigidbody())
            {
                _body.MoveRotation(Quaternion.LookRotation(value));

            }
            else
            {
                transform.forward = value;
            }
        }
    }

    public float Mass
    {
        get
        {
            if (_body != null) return _body.mass;
            else return 1f;
        }
    }

    private bool UsingRigidbody()
    {
        switch (manipulatingStyle)
        {
            case ManipulatingStyle.Transform:
                return false;
            default:
                return _body != null;
        }
    }

    public void SetPosition(Vector3 pos)
    {
        if(UsingRigidbody())
        {
            _body.position = pos;
        }
        else
        {
            transform.position = pos;
        }
    }

    public void SetRotation(Vector3 dir)
    {
        if(UsingRigidbody())
        {
            _body.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            transform.forward = dir;
        }
    }
    



    public void SetVelocity(Vector3 vel)
    {
        //if (vel != _currentInputVelocity) Debug.LogFormat("Setting Velocity of {0} as {1}", transform.root, vel.ToString("F3"));
        _currentInputVelocity = vel;
    }

    public void AddVelocitySource(IKinematicPlatformerVelocityGetter getter, int priority = 0)
    {
        Sources[priority].AddLast(getter);
    }

    public void RemoveVelocitySource(IKinematicPlatformerVelocityGetter getter, int priority = 0)
    {
        Sources[priority].Remove(getter);
    }

    public void AddVelocitySource(IKinematicPlatformerFrameVelocityGetter getter, int priority = 0)
    {
        FrameSources[priority].AddLast(getter);
        //Debug.LogFormat("Added {0} and has {1}", getter, Sources.Count);
    }

    public void RemoveVelocitySource(IKinematicPlatformerFrameVelocityGetter getter, int priority = 0)
    {
        FrameSources[priority].Remove(getter);
        //Debug.LogFormat("Removed {0} and has {1}", getter, Sources.Count);
    }

    public void AddVelocity(Vector3 vel)
    {
        _currentInputVelocity += vel;
    }

    public void CancelVelocity()
    {
        _currentInputVelocity = Vector3.zero;
    }

    public void AddExactNextFrameMove(Vector3 move, int priority)
    {
#if UNITY_EDITOR
        if(move.IsNaN())
        {
            Debug.LogErrorFormat("{0} += {1}! Putting NaN in this!", _setFrameMovement, move);
        }
#endif

        if (_setFrameMovement == null) _setFrameMovement = new Dictionary<int, Vector3>(2);
        if (!_setFrameMovement.ContainsKey(priority)) _setFrameMovement.Add(priority, move);
        else
        {
            _setFrameMovement[priority] += move;
        }
    }

    private Vector3 ExtractNextFrameMove(float deltaTime)
    {
        Vector3 setFrameMovement = ExtractNextFrameMoveFromSetFrameMove(out int frameSetPriority);
        CleanNextSetFrameMove();

        Vector3 sourcesMovement = ExtractNextFrameMoveFromSources(out int sourcesPriority, deltaTime);

        if (sourcesPriority == frameSetPriority) return setFrameMovement + sourcesMovement;
        else if (sourcesPriority > frameSetPriority) return sourcesMovement;
        else return setFrameMovement;
    }

    private Vector3 ExtractNextFrameMoveFromSetFrameMove(out int biggestPriority)
    {
        if (_setFrameMovement == null)
        {
            biggestPriority = int.MinValue;
            return Vector3.zero;
        }
        biggestPriority = _setFrameMovement.Max((x) => x.Key);
        return _setFrameMovement[biggestPriority];
    }

    private Vector3 ExtractNextFrameMoveFromSources(out int priority, float deltaTime)
    {
        Vector3 frameSourcesMovement = Vector3.zero;
        int latestPriority = FrameSources.GetLatestPriority();
        var list = FrameSources.GetBiggestPriorityList(out priority);
        if(list != null)
        {
            foreach (IKinematicPlatformerFrameVelocityGetter getter in list)
            {
                if (!getter.Equals(null))
                {
                    if (latestPriority != priority && getter is IKinematicPlatformerVelocityGetterActivateable) 
                        (getter as IKinematicPlatformerVelocityGetterActivateable).StartVelocity();
                    Vector3 velFrame = getter.GetFrameVelocity(deltaTime);
                    if (velFrame != Vector3.zero)
                    {
                        frameSourcesMovement += velFrame;
                    }
                }
            }
        }
        return frameSourcesMovement;
    }

    private void CleanNextSetFrameMove()
    {
        if(_setFrameMovement != null)
        {
            foreach(var x in _setFrameMovement.Keys.ToList())
            {
                _setFrameMovement[x] = Vector3.zero;
            }
        }
    }

    protected bool IsGrounded()
    {
        return Grounded;
    }


    public Vector3 AbsoluteVelocity
    {
        get
        {
            return _latestValidVelocity / Time.fixedDeltaTime;
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return AbsoluteVelocity - _outsideVelocity;
        }
    }

    public Vector3 LatestNonZeroVelocity
    {
        get
        {
            return _latestNonZeroValidMovement / Time.fixedDeltaTime;
        }
    }

    public class ArbitraryCheckData
    {
        public Vector3 direction;
        public float length;
        public CasterClass caster;
        public bool consumable;
    }

    private Dictionary<CasterClass, List<ArbitraryCheckData>> _arbitraryChecks;

    public ArbitraryCheckData AddArbitraryCheck(in Vector3 direction, float length, CasterClass caster)
    {
        ArbitraryCheckData data = new ArbitraryCheckData()
        {
            direction = direction,
            length = length,
            caster = caster,
            consumable = false
        };
        if (_arbitraryChecks == null) _arbitraryChecks = new Dictionary<CasterClass, List<ArbitraryCheckData>>((int)CasterClass.None);
        if (!_arbitraryChecks.ContainsKey(caster)) _arbitraryChecks.Add(caster, new List<ArbitraryCheckData>(4));
        _arbitraryChecks[caster].Add(data);
        return data;
    }

    public void AddArbitraryCheckConsumable(in Vector3 direction, float length, CasterClass caster)
    {
        ArbitraryCheckData data = new ArbitraryCheckData()
        {
            direction = direction,
            length = length,
            caster = caster,
            consumable = true
        };
        if (_arbitraryChecks == null) _arbitraryChecks = new Dictionary<CasterClass, List<ArbitraryCheckData>>((int)CasterClass.None);
        if (!_arbitraryChecks.ContainsKey(caster)) _arbitraryChecks.Add(caster, new List<ArbitraryCheckData>(4));
        _arbitraryChecks[caster].Add(data);
    }

    public void RemoveArbitraryCheck(CasterClass caster, ref ArbitraryCheckData data)
    {
        if (_arbitraryChecks == null || !_arbitraryChecks.ContainsKey(caster)) return;
        _arbitraryChecks[caster].Remove(data);
        data = null;
    }

    /// <summary> 
    /// Check certain arbitrary checks for a caster, and return the movement needed for those.
    /// </summary>

    private Vector3 CheckArbitraryChecks(in Vector3 offset, CasterClass caster)
    {
        if (_arbitraryChecks == null || !_arbitraryChecks.ContainsKey(caster)) return Vector3.zero;

        List<ArbitraryCheckData> list = _arbitraryChecks[caster];

        if (list.Count <= 0) return Vector3.zero;

        Caster casterObj = GetCaster(caster);
        Vector3 totalMove = Vector3.zero;

        Debug.LogWarningFormat(this,"Checking out all the {0} contacts on {1}.", list.Count, caster);

        int i = 0;
        while(i < list.Count)
        {
            ArbitraryCheckData check = list[i];
            if (this.CastLengthOffsetInFrame(casterObj, offset, check.direction, check.length, out RaycastHit hit, comment: "Arbitrary check asked from outside"))
            {
                totalMove += check.direction.normalized * hit.distance;
            }
            Debug.LogWarningFormat(this, "Got the total amount {0} from direction {1}. Cast result is '{2}'", totalMove.ToString("F3"), check.direction, hit.collider);
            if (check.consumable)
            {
                list.Remove(check);
                Debug.LogWarningFormat(this, "Removing now have {0} casts.", list.Count, caster);
                continue;
            }
            i++;
        }

        return totalMove;
    }

    private Dictionary<CasterClass, Vector3> _accumulatedValue;

    public void CheckSurfaceNow(CasterClass caster, in Vector3 offset, in Vector3 direction, float length, Vector3 extraValueIfOn)
    {
        if (direction == Vector3.zero) return;
        Caster casterObj = GetCaster(caster);


        if (this.CastLengthOffsetInFrame(casterObj, offset, direction, length, out RaycastHit hit, comment: "Immediate check on incoming surface"))
        {
            AddAccumulatedValue(caster, direction.normalized * hit.distance + extraValueIfOn);
        }
    }

    private void AddAccumulatedValue(CasterClass caster, Vector3 v)
    {
        if (_accumulatedValue == null) _accumulatedValue = new Dictionary<CasterClass, Vector3>((int) CasterClass.None);
        if (!_accumulatedValue.ContainsKey(caster)) _accumulatedValue.Add(caster, Vector3.zero);

        _accumulatedValue[caster] += v;
    }

    private Vector3 CheckAccumulatedValue(CasterClass caster, bool destroy = true)
    {
        if (_accumulatedValue == null || !_accumulatedValue.ContainsKey(caster)) return Vector3.zero;
        Vector3 v = _accumulatedValue[caster];
        if(destroy) _accumulatedValue[caster] = Vector3.zero;
        return v;
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
            int latestPriority = _velocitySources.GetLatestPriority();
            var list = _velocitySources.GetBiggestPriorityList(out int biggestPriority);
            if(list != null)
            {
                foreach (IKinematicPlatformerVelocityGetter getter in list)
                {
                    if (latestPriority != biggestPriority && getter is IKinematicPlatformerVelocityGetterActivateable) (getter as IKinematicPlatformerVelocityGetterActivateable).StartVelocity();
                    sourceVel += getter.GetVelocity();
                }
            }
        }
#if UNITY_EDITOR
        _debugCurrentOtherSourcesInputVelocity = sourceVel;
#endif
        return _currentInputVelocity + sourceVel;
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR 
        history = new CastHistory();
#endif

        Vector3 totalMovement = GetCurrentVelocity();
        
        Vector3 extractMovement = ExtractNextFrameMove(Time.fixedDeltaTime);
        Vector3 frameMovement = totalMovement * Time.fixedDeltaTime + extractMovement;
#if UNITY_EDITOR
        _lastTotalVelDebug = frameMovement / Time.fixedDeltaTime;
#endif
        Vector3 horizontalChecks = CheckAccumulatedValue(CasterClass.Side);
        Vector3 verticalChecks = CheckAccumulatedValue(CasterClass.Ground) + CheckAccumulatedValue(CasterClass.Ceiling);
        frameMovement += horizontalChecks + verticalChecks;

        CheckPlatformMovement(_currentPlatform, out Vector3 platformMovement, out Quaternion platformRotation);
        SaveRelativeSpeed(platformMovement / Time.fixedDeltaTime);
        frameMovement += platformMovement;

        Vector3 horizontalMovement = GetHorizontalMovement(frameMovement);
        Vector3 verticalMovement = GetVerticalMovement(frameMovement);

        Vector3 movementMade = Vector3.zero;

        bool nowGrounded = Grounded, nowHittingHead = HittingHead, nowWalled = Walled;
        //Horizontal movement resolve
        CheckAndReflectMovement(wallCaster, movementMade, ref horizontalMovement, out Vector3 lateralReflection, out Vector3 lateralDrag, out nowWalled, out RaycastHit newWall);
        //Vector3 horizontalChecks = CheckAccumulatedValue(CasterClass.Side);

        //Divide vertical movement and add total horizontal movement into final movement.
        Vector3 totalHorizontalMovement = horizontalMovement + lateralDrag;
        verticalMovement += GetVerticalMovement(totalHorizontalMovement);
        movementMade += GetHorizontalMovement(totalHorizontalMovement);

        //After moving horizontally, then checks the ground so players can squeeze better through places.
        Vector3 verticalReflection = Vector3.zero;
        RaycastHit newCeilingHit = new RaycastHit(), newPlatformHit = new RaycastHit();
        if (verticalMovement.y > 0f) CheckAndStopMovement(ceilingCaster, movementMade, ref verticalMovement, out verticalReflection, out nowHittingHead, out newCeilingHit);
        else CheckAndStopMovement(groundCaster, movementMade, ref verticalMovement, out verticalReflection, out nowGrounded, out newPlatformHit);

        movementMade += verticalMovement;

        //Vector3 verticalChecks = CheckAccumulatedValue(CasterClass.Ground) + CheckAccumulatedValue(CasterClass.Ceiling);
        //movementMade += verticalChecks;


#if UNITY_EDITOR
        if (movementMade.IsNaN())
        {
            Debug.LogErrorFormat(this, "[PLATFORMER] {0} NAN ALERT -> mov:{1} vert:{2} horiz:{3} horizChecks:{4} frame:{5} extract:{6} total:{7}. Delta time is {8}", this, movementMade, verticalMovement,
                horizontalMovement, horizontalChecks, frameMovement, extractMovement, totalMovement, Time.fixedDeltaTime);
            return;
        }
#endif

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


        Direction = platformRotation * Direction;
        Move(movementMade, out Vector3 positionResult);
        SyncRotation(_desiredDirection);
        UpdatePlatform(newPlatformHit, positionResult);

#if UNITY_EDITOR
        _debugGrounded = Grounded;
        _debugHittingHead = HittingHead;
        _debugWalled = Walled;

        if(Debugging && history.elements.Count > ((int)CasterClass.None - 1))
        {
            Debug.LogWarningFormat(this, "There has been {0} casts on fixed frame {1} [part of frame {2}].", history.elements.Count, Time.fixedTime, Time.frameCount);
            int i = 1;
            foreach (var elem in history) Debug.LogFormat(this, "Cast from {0} to {1} using {2}. It hit '{3}'. (Reason: {4}) [Cast {5}]", elem.offset.ToString("F3"), elem.distance.ToString("F3"), elem.caster, elem.hit.collider, elem.comment, i++);
        }
#endif
    }

    private Vector3 GetHorizontalMovement(in Vector3 totalMovement)
    {
        return Vector3.ProjectOnPlane(totalMovement, Vector3.up);
    }

    private Vector3 GetVerticalMovement(in Vector3 totalMovement)
    {
        return Vector3.Project(totalMovement, Vector3.up);
    }

    private void CheckAndReflectMovement(Caster caster, in Vector3 originOffset, ref Vector3 movement, out Vector3 reflection, out Vector3 drag, out bool foundCast, out RaycastHit foundHit)
    {
        foundCast = false;
        foundHit = default(RaycastHit);
        Vector3 currentMovement = movement;
        Vector3 extraMovement = Vector3.zero;
        Vector3 originalMovement = movement;
        Vector3 origin = originOffset;
        reflection = Vector3.zero;
        drag = Vector3.zero;
        if (caster != null)
        {
            float moveMagnitude = currentMovement.magnitude;
            if (currentMovement.sqrMagnitude > SMALL_AMOUNT_SQRD && CastLengthOffsetInFrame(caster, origin, currentMovement, moveMagnitude, out foundHit, comment: "CheckAndReflect new"))
            {
                foundCast = true;

                Vector3 hugWall = currentMovement.normalized * foundHit.distance;
                if (hugWall.sqrMagnitude < SMALL_AMOUNT_SQRD) hugWall = Vector3.zero;
                origin += hugWall;
                extraMovement = currentMovement - hugWall;
                currentMovement = hugWall;

                //Old hug wall algorthm
                //currentMovement = caster.GetCenterPositionOfHit(hitFirst) - caster.GetOriginPositionOffset(originOffset); //Hug the wall
                //extraMovement = originalMovement.normalized * (moveMagnitude - currentMovement.magnitude);

                //Excess movement
                reflection += Vector3.Reflect(extraMovement, foundHit.normal) * reflectionMultiplier;
                drag = Vector3.ProjectOnPlane(extraMovement, foundHit.normal) * dragMultiplier;
                //If drag will stop at a wall, stop it.
                if (drag.sqrMagnitude > SMALL_AMOUNT_SQRD && CastLengthOffsetInFrame(caster, origin, drag, drag.magnitude, out RaycastHit hitSecond, comment: "Drag movement check"))
                {
                    //Sum with the distances to actually get the intersection between the planes where the distances are already considering where the caster should be
                    Vector3 plane1Pos = foundHit.point + caster.GetMinimumDistanceFromHit(foundHit.point, foundHit.normal);
                    Vector3 plane2Pos = hitSecond.point + caster.GetMinimumDistanceFromHit(hitSecond.point, hitSecond.normal);
                    Vector3 pointBetween = GetPointBetweenTwoPlanes(plane1Pos, plane2Pos, foundHit.normal, hitSecond.normal);

                    Vector3 originalDrag = drag;
                    drag = pointBetween - (caster.GetOriginPositionOffset(originOffset) + currentMovement);
                    if (Debugging)
                    {
                        LHH.Utils.DebugUtils.DrawNormalStar(pointBetween, 1f, Quaternion.identity, Color.yellow, 10f);
                        LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset), caster.GetCenterPositionOfHit(foundHit), 1f, 45f, Color.green, 0f);
                        LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset + currentMovement), pointBetween, 1f, 45f, Color.red, 0f);
                        Debug.LogFormat(this, "{8} Yes Hit Drag! {0} is now movement {1} with extra {2} [dist:{6}, moveMag:{7}] (drag {3} (was {4}) + reflect {5}) --> {9} Will end up on {10}", originalMovement.ToString("F3"), currentMovement.ToString("F3"), extraMovement.ToString("F3"), 
                            drag.ToString("F3"), originalDrag.ToString("F3"),
                            reflection.ToString("F3"), foundHit.distance, moveMagnitude, Time.fixedTime,
                            Position.ToString("F3"), (Position + currentMovement + drag).ToString("F3"));
                    }
                }
                else
                {
                    if (Debugging)
                    {
                        LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset), caster.GetCenterPositionOfHit(foundHit), 1f, 45f, Color.green, 0f);
                        LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset) + currentMovement, caster.GetOriginPositionOffset(originOffset) + currentMovement + drag, 1f, 45f, Color.red, 0f); 
                        Debug.LogFormat(this, "{8} No Hit Drag! {0} is now movement {1} with extra {2} [dist:{6}, moveMag:{7}] (drag {3} (was {4}) + reflect {5}) --> {9} Will end up on {10}", originalMovement.ToString("F3"), currentMovement.ToString("F3"), extraMovement.ToString("F3"),
                                    drag.ToString("F3"), drag.ToString("F3"),
                                    reflection.ToString("F3"), foundHit.distance, moveMagnitude, Time.fixedTime,
                                    Position.ToString("F3"), (Position + currentMovement + drag).ToString("F3"));
                    }
                }

                movement = currentMovement;
            }
        }
    }



    private Vector3 GetPointBetweenTwoPlanes(in Vector3 plane1Pos, in Vector3 plane2Pos, in Vector3 plane1Normal, in Vector3 plane2Normal)
    {
        //The line of the intersection between the two planes
        Vector3 lineIntersection = Vector3.Cross(plane1Normal, plane2Normal);
        //Discover a direction alongside plane that we can make a parametric equation with
        Vector3 directionAlongsidePlaneHit = Vector3.Cross(plane2Normal, lineIntersection);
        float numerator = Vector3.Dot(plane1Normal, directionAlongsidePlaneHit);
        Vector3 plane1ToPlane2 = plane1Pos - plane2Pos;
        float t = Vector3.Dot(plane1Normal, plane1ToPlane2) / numerator; //Discover where does the planes intersect parametrically (I think? Need to study)
        Vector3 pointBetween = plane2Pos + t * directionAlongsidePlaneHit; // Go alonside the plane 2 with the direction to the intersection
        return pointBetween;
    }

    private void CheckAndStopMovement(Caster caster, in Vector3 originOffset, ref Vector3 movement, out Vector3 reflection, out bool foundCast, out RaycastHit foundHit)
    {
        foundCast = false;
        foundHit = default(RaycastHit);
        reflection = Vector3.zero;
        if (caster != null)
        {
            float moveMagnitude = movement.magnitude;
            if (movement != Vector3.zero && CastLengthOffsetInFrame(caster, originOffset, movement, moveMagnitude, out foundHit, "Check and stop movement"))
            {
                if (foundHit.distance < moveMagnitude)
                {
                    foundCast = true; //If movement is going to end up inside the collider, it is a solid hit
                    Vector3 amountToHugWall = movement.normalized * foundHit.distance;
                    reflection += Vector3.ProjectOnPlane(movement - amountToHugWall, foundHit.normal);
                    movement = amountToHugWall;
                }
            }
        }
    }

    /// <summary>
    /// Cast private wrapper for debug reasons.
    /// </summary>
    /// <returns></returns>
    private bool CastLengthOffsetInFrame(Caster caster, in Vector3 offset, in Vector3 direction, float length, out RaycastHit hit, string comment)
    {
        bool success = caster.CastLengthOffset(offset, direction, length, out hit);
#if UNITY_EDITOR
        history.Add(new CastHistoryElement()
        {
            hit = hit,
            caster = caster,
            offset = offset,
            distance = direction,
            comment = comment
        });
#endif
        return success;
    }

    public Collider WhatIsWhereIAmTryingToGo(CasterClass caster, out RaycastHit hit)
    {
        return WhatIsInThere(GetCurrentVelocity(), Vector3.zero, caster, out hit);
    }

    public Collider WhatIsInThere(in Vector3 checkDistance, in Vector3 offset, CasterClass caster, out RaycastHit hit)
    {
        return WhatIsInThere(checkDistance, offset, GetCaster(caster),out hit);
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

    private Collider WhatIsInThere(in Vector3 checkDistance, in Vector3 offset, Caster caster, out RaycastHit hit)
    {
        hit = default(RaycastHit);
        if (caster != null && CastLengthOffsetInFrame(caster, offset, checkDistance, checkDistance.magnitude, out hit, "What is in there"))
        {
            return hit.collider;
        }
        else return null;
    }


    private void SolveReflection(in Vector3 inVel, float minVelToReflect, float reflectionAbsortion)
    {

    }



    private void Move(in Vector3 movement, out Vector3 positionNow)
    {
        if (movement.sqrMagnitude != 0f) 
            _latestNonZeroValidMovement = movement;

        _latestValidVelocity = movement;
#if UNITY_EDITOR
        _lastValidVelDebug = _latestValidVelocity / Time.fixedDeltaTime;
        //if (name.Contains("Player")) Debug.LogFormat(this, "Movement is {0} and Last val debug is {1}", movement.ToString("F4"), _lastValidVelDebug.ToString("F4"));
#endif
        if (movement.sqrMagnitude > SMALL_AMOUNT_SQRD)
        {
            if (Debugging)
                Debug.LogFormat("{0} going from {1} with {2} = {3}", 
                    this, Position.ToString("F4"), movement.ToString("F4"), (Position + movement).ToString("F4"));

            positionNow = Position + movement;
            Position = positionNow;
        }
        else
        {
            positionNow = Position;
        }
    }

    private void SyncRotation(in Vector3 direction)
    {
        SetActualDirection(direction);
    }

    private void UpdatePlatform(RaycastHit hit, in Vector3 positionNow)
    {
        if(_currentPlatform.platform != hit.collider) //Just changed
        {
            if(hit.collider != null)
            {
                _currentPlatform.platform = hit.collider;
            }
            else
            {
                _currentPlatform.platform = null;
            }

            JustChangedPlatform(_currentPlatform);
        }

        //Update values anyway
        if(_currentPlatform.platform != null)
        {
            _currentPlatform.currentRotation = _currentPlatform.platform.transform.rotation;
            _currentPlatform.untransformedContactPoint = _currentPlatform.platform.transform.InverseTransformPoint(positionNow);
        }
    }

    private void CheckPlatformMovement(in PlatformState state, out Vector3 platformMovement, out Quaternion platformRotation)
    {
        platformMovement = Vector3.zero;
        platformRotation = Quaternion.identity;
        if(state.platform != null)
        {
            platformMovement = state.platform.transform.TransformPoint(state.untransformedContactPoint) - Position;
            platformRotation = Quaternion.Inverse(state.currentRotation) * state.platform.transform.rotation;
        }
    }

    private void JustChangedPlatform(PlatformState platform)
    {
    }

    private void SaveRelativeSpeed(Vector3 relativeSpeed)
    {
        _outsideVelocity = relativeSpeed;
    }
}



