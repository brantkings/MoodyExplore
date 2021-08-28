using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;
using System.Linq;

public interface IKinematicForceGetter
{
    void SetGettingForce(bool set);
}

public interface IKinematicPlatformerVelocityGetter
{
    Vector3 GetVelocity();
}
public interface IKinematicPlatformerFrameVelocityGetter
{
    Vector3 GetFrameVelocity(float deltaTime);
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

    public bool _hasGravity;

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
    private Vector3 _setFrameMovement;


    private Vector3 _latestNonZeroValidMovement;
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

    private class VelocityGetter<T> : IEnumerable<T>
    {
        public Dictionary<int, LinkedList<T>> _values;

        public T Current => throw new System.NotImplementedException();

        public VelocityGetter()
        {
            _values = new Dictionary<int, LinkedList<T>>(8);
        }


        public LinkedList<T> this[int key]
        {
            get
            {
                if (!_values.ContainsKey(key))
                {
                    _values.Add(key, new LinkedList<T>());
                }
                return _values[key];
            }
        }

        private KeyValuePair<int, LinkedList<T>> FindMaxValue(KeyValuePair<int, LinkedList<T>> x, KeyValuePair<int, LinkedList<T>> y)
        {
            return x.Key > y.Key ? x : y;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if(_values.Count > 0)
            {
                KeyValuePair<int, LinkedList<T>> r = _values.Aggregate(FindMaxValue);
                if (r.Value != null)
                {
                    LinkedListNode<T> node = r.Value.First;
                    while (node != null)
                    {
                        yield return node.Value;
                        node = node.Next;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_values.Count > 0)
            {
                KeyValuePair<int, LinkedList<T>> r = _values.Aggregate(FindMaxValue);
                if (r.Value != null)
                {
                    LinkedListNode<T> node = r.Value.First;
                    while (node != null)
                    {
                        yield return node.Value;
                        node = node.Next;
                    }
                }
            }
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
            if (_velocitySources == null) _frameVelocitySources = new VelocityGetter<IKinematicPlatformerFrameVelocityGetter>();
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
        get => transform.forward;
        set
        {
            if(value.sqrMagnitude != 0f)
            {
                if(UsingRigidbody())
                {
                    _body.MoveRotation(Quaternion.LookRotation(value));

                }
                else
                {
                    transform.forward = value;
                }
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

    public void AddExactNextFrameMove(Vector3 move)
    {
#if UNITY_EDITOR
        if(move.IsNaN())
        {
            Debug.LogErrorFormat("{0} += {1}! Putting NaN in this!", _setFrameMovement, move);
        }
#endif
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

    public Vector3 LatestNonZeroVelocity
    {
        get
        {
            return _latestNonZeroValidMovement / Time.fixedDeltaTime;
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
        _debugCurrentOtherSourcesInputVelocity = sourceVel;
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
#if UNITY_EDITOR 
        history = new CastHistory();
#endif

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
        CheckAndReflectMovement(ref horizontalMovement, out Vector3 lateralReflection, out Vector3 lateralDrag, out nowWalled, movementMade, wallCaster);

        if (Debugging && (horizontalMovement + lateralDrag) != Vector3.zero)
        {
            Debug.LogWarningFormat(this, "{0} + {1} = {2}", horizontalMovement.ToString("F3"), lateralDrag.ToString("F3"), (horizontalMovement + lateralDrag).ToString("F3"));
            LHH.Utils.DebugUtils.DrawNormalStar(Position + horizontalMovement + lateralDrag, 0.5f, Quaternion.identity, Color.green, 0f);
            Debug.DebugBreak();
        }
        //Divide vertical movement and add total horizontal movement into final movement.
        Vector3 totalHorizontalMovement = horizontalMovement + lateralDrag;
        verticalMovement += GetVerticalMovement(totalHorizontalMovement);
        movementMade += GetHorizontalMovement(totalHorizontalMovement);

        //After moving horizontally, then checks the ground so players can squeeze better through places.
        Vector3 verticalReflection = Vector3.zero;
        if (verticalMovement.y > 0f) CheckAndStopMovement(ref verticalMovement, out verticalReflection, out nowHittingHead, movementMade, ceilingCaster);
        else CheckAndStopMovement(ref verticalMovement, out verticalReflection, out nowGrounded, movementMade, groundCaster);

        movementMade += verticalMovement;

#if UNITY_EDITOR
        if (movementMade.IsNaN())
        {
            Debug.LogErrorFormat(this, "[PLATFORMER] {0} NAN ALERT -> mov:{1} vert:{2} horiz:{3} frame:{4} extract:{5} total:{6}. Delta time is {7}", this, movementMade, verticalMovement, horizontalMovement, frameMovement, extractMovement, totalMovement, Time.fixedDeltaTime);
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
        

        Move(movementMade);

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

    private Vector3 ApproachColliderThroughNormal(Caster caster, in RaycastHit hit, in Vector3 originOffset, in Vector3 movementToHitWall)
    { 
        Vector3 totalMovement = movementToHitWall;
        Vector3 lateralAmountToHugWall = Vector3.Project(movementToHitWall, hit.normal); //This didn't let corners have different resting positions for different forces.
        Vector3 origin = originOffset + movementToHitWall;


#if UNITY_EDITOR
        int count = 0;
#endif
        while (lateralAmountToHugWall.sqrMagnitude > SMALL_AMOUNT_SQRD && CastLengthOffsetInFrame(caster, origin, lateralAmountToHugWall, out RaycastHit precisionHit, comment: "Approach through normal"))
        {
            lateralAmountToHugWall = lateralAmountToHugWall.normalized * precisionHit.distance;
            totalMovement += lateralAmountToHugWall;
            origin += lateralAmountToHugWall;

#if UNITY_EDITOR
            if (count++ > 10)
            {
                Debug.LogErrorFormat(this, "Possible infinite loop detected: {0} to {1} in hug collider approaching normal.", movementToHitWall.ToString("F4"), precisionHit.collider);
                break;
            }
#endif
        }
        return totalMovement;
    }

    private void CheckAndReflectMovementOld(ref Vector3 movement, out Vector3 reflection, out bool foundCast, in Vector3 originOffset, Caster caster, bool hugColliderApproachingNormal)
    {
        foundCast = false;
        reflection = Vector3.zero;
        if (caster != null)
        {
            Vector3 oldMove = movement;
            //Debug.LogFormat("[{0}] Checking {1} with offset {2} ({3}) ", Time.frameCount, movement.ToString("F3"), originOffset.ToString("F3"), caster);

            Vector3 movementToGlueWithWall = Vector3.zero;
            int numberOfHits = 0;
            while (movement != Vector3.zero && CastLengthOffsetInFrame(caster, originOffset, movement, out RaycastHit hit, comment: "Check and reflect movement"))
            {
                float moveMagnitude = movement.magnitude;
                if (hit.distance < moveMagnitude) foundCast = true; //If movement is going to end up inside the collider, it is a solid hit

                Vector3 amountToHugWall = movement.normalized * Mathf.Min(hit.distance, moveMagnitude); //Hug the wall.
                Vector3 toGlueWithWallThisTime;
                if (hugColliderApproachingNormal)
                    toGlueWithWallThisTime = ApproachColliderThroughNormal(caster, hit, originOffset, amountToHugWall); //With this on, at least two casts will be done
                else
                    toGlueWithWallThisTime = amountToHugWall;

#if UNITY_EDITOR
                if (movement.IsNaN() || reflection.IsNaN() || movementToGlueWithWall.IsNaN() || toGlueWithWallThisTime.IsNaN())
                {
                    Debug.LogErrorFormat(this, "[PLATFORMER] {0} NAN ALERT -> mov:{1} ref:{2} movToGlue:{3} movToGlueNow:{4}", this, movement, reflection, movementToGlueWithWall, toGlueWithWallThisTime);
                }
#endif
                movementToGlueWithWall += toGlueWithWallThisTime;
                reflection += Vector3.Project(movement - toGlueWithWallThisTime, hit.normal);

                movement = Vector3.ProjectOnPlane(movement, hit.normal);

                if (++numberOfHits > 10)
                {
                    Debug.LogErrorFormat(this, "Infinite loop! {0} {1}", hit.normal, hit.collider);
                    break;
                }
            }

            movement += movementToGlueWithWall;


        }
    }

    private void CheckAndReflectMovement(ref Vector3 movement, out Vector3 reflection, out Vector3 drag, out bool foundCast, in Vector3 originOffset, Caster caster)
    {
        foundCast = false;
        Vector3 currentMovement = movement;
        Vector3 extraMovement = Vector3.zero;
        Vector3 originalMovement = movement;
        Vector3 origin = originOffset;
        reflection = Vector3.zero;
        drag = Vector3.zero;
        if (caster != null)
        {
            if(currentMovement.sqrMagnitude > SMALL_AMOUNT_SQRD && CastLengthOffsetInFrame(caster, origin, currentMovement, out RaycastHit hitFirst, comment: "CheckAndReflect new"))
            {
                float moveMagnitude = currentMovement.magnitude;
                //if (hitFirst.distance < moveMagnitude) //If movement is going to end up inside the collider, it is a solid hit
                //{
                    foundCast = true;

                    Vector3 hugWall = currentMovement.normalized * hitFirst.distance;
                    if (hugWall.sqrMagnitude < SMALL_AMOUNT_SQRD) hugWall = Vector3.zero;
                    origin += hugWall;
                    extraMovement = currentMovement - hugWall;
                    currentMovement = hugWall;

                    //Old hug wall algorthm
                    //currentMovement = caster.GetCenterPositionOfHit(hitFirst) - caster.GetOriginPositionOffset(originOffset); //Hug the wall
                    //extraMovement = originalMovement.normalized * (moveMagnitude - currentMovement.magnitude);

                    //Excess movement
                    reflection += Vector3.Reflect(extraMovement, hitFirst.normal) * reflectionMultiplier;
                    drag = Vector3.ProjectOnPlane(extraMovement, hitFirst.normal) * dragMultiplier;
                    //If drag will stop at a wall, stop it.
                    if (drag.sqrMagnitude > SMALL_AMOUNT_SQRD && CastLengthOffsetInFrame(caster, origin, drag, out RaycastHit hitSecond, comment: "Drag movement check"))
                    {
                        //Sum with the distances to actually get the intersection between the planes where the distances are already considering where the caster should be
                        Vector3 plane1Pos = hitFirst.point + caster.GetMinimumDistanceFromHit(hitFirst.normal);
                        Vector3 plane2Pos = hitSecond.point + caster.GetMinimumDistanceFromHit(hitSecond.normal);
                        //The line of the intersection between the two planes
                        Vector3 lineIntersection = Vector3.Cross(hitFirst.normal, hitSecond.normal);
                        //Discover a direction alongside plane that we can make a parametric equation with
                        Vector3 directionAlongsidePlaneHit = Vector3.Cross(hitSecond.normal, lineIntersection);
                        float numerator = Vector3.Dot(hitFirst.normal, directionAlongsidePlaneHit);
                        Vector3 plane1ToPlane2 = plane1Pos - plane2Pos;
                        float t = Vector3.Dot(hitFirst.normal, plane1ToPlane2) / numerator; //Discover where does the planes intersect parametrically (I think? Need to study)
                        Vector3 pointBetween = plane2Pos + t * directionAlongsidePlaneHit; // Go alonside the plane 2 with the direction to the intersection

                        Vector3 originalDrag = drag;
                        drag = pointBetween - (caster.GetOriginPositionOffset(originOffset) + currentMovement);
                        if (Debugging)
                        {
                            LHH.Utils.DebugUtils.DrawNormalStar(pointBetween, 1f, Quaternion.identity, Color.yellow, 10f);
                            LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset), caster.GetCenterPositionOfHit(hitFirst), 1f, 45f, Color.green, 0f);
                            LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset + currentMovement), pointBetween, 1f, 45f, Color.red, 0f);
                            Debug.LogFormat(this, "{8} Yes Hit Drag! {0} is now movement {1} with extra {2} [dist:{6}, moveMag:{7}] (drag {3} (was {4}) + reflect {5}) --> {9} Will end up on {10}", originalMovement.ToString("F3"), currentMovement.ToString("F3"), extraMovement.ToString("F3"), 
                                drag.ToString("F3"), originalDrag.ToString("F3"),
                                reflection.ToString("F3"), hitFirst.distance, moveMagnitude, Time.fixedTime,
                                Position.ToString("F3"), (Position + currentMovement + drag).ToString("F3"));
                        }
                    }
                    else
                    {
                        if (Debugging)
                        {
                            LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset), caster.GetCenterPositionOfHit(hitFirst), 1f, 45f, Color.green, 0f);
                            LHH.Utils.DebugUtils.DrawArrow(caster.GetOriginPositionOffset(originOffset) + currentMovement, caster.GetOriginPositionOffset(originOffset) + currentMovement + drag, 1f, 45f, Color.red, 0f); 
                            Debug.LogFormat(this, "{8} No Hit Drag! {0} is now movement {1} with extra {2} [dist:{6}, moveMag:{7}] (drag {3} (was {4}) + reflect {5}) --> {9} Will end up on {10}", originalMovement.ToString("F3"), currentMovement.ToString("F3"), extraMovement.ToString("F3"),
                                     drag.ToString("F3"), drag.ToString("F3"),
                                     reflection.ToString("F3"), hitFirst.distance, moveMagnitude, Time.fixedTime,
                                     Position.ToString("F3"), (Position + currentMovement + drag).ToString("F3"));
                        }
                    }

                    movement = currentMovement;
                //}
            }
            
            
        }
    }

    /// <summary>
    /// Cast private wrapper for debug reasons.
    /// </summary>
    /// <returns></returns>
    private bool CastLengthOffsetInFrame(Caster caster, in Vector3 offset, in Vector3 distance, out RaycastHit hit, string comment)
    {
        bool success = caster.CastLengthOffset(offset, distance, out hit);
#if UNITY_EDITOR
        history.Add(new CastHistoryElement()
        {
            hit = hit,
            caster = caster,
            offset = offset,
            distance = distance,
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
        if (caster != null && CastLengthOffsetInFrame(caster, offset, checkDistance, out hit, "What is in there"))
        {
            return hit.collider;
        }
        else return null;
    }

    private void CheckAndStopMovement(ref Vector3 movement, out Vector3 reflection, out bool foundCast, in Vector3 originOffset, Caster caster)
    {
        foundCast = false;
        reflection = Vector3.zero;
        if (caster != null)
        {
            Vector3 oldMove = movement;
            if (movement != Vector3.zero && CastLengthOffsetInFrame(caster, originOffset, movement, out RaycastHit hit, "Check and stop movement"))
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

    private void SolveReflection(in Vector3 inVel, float minVelToReflect, float reflectionAbsortion)
    {

    }



    private void Move(in Vector3 movement)
    {
        if (movement.sqrMagnitude != 0f) 
            _latestNonZeroValidMovement = movement;

        _latestValidVelocity = movement;
#if UNITY_EDITOR
        _lastValidVelDebug = _latestValidVelocity / Time.fixedDeltaTime;
        //if (name.Contains("Player")) Debug.LogFormat(this, "Movement is {0} and Last val debug is {1}", movement.ToString("F4"), _lastValidVelDebug.ToString("F4"));
#endif
        if (movement != Vector3.zero)
        {
            if (Debugging)
                Debug.LogFormat("{0} going from {1} with {2} = {3}", 
                    this, Position.ToString("F4"), movement.ToString("F4"), (Position + movement).ToString("F4"));
            Position = Position + movement;
        }
    }
}



