using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public delegate void DelPlatformerEvent();
public delegate void DelPlatformerConditionalEvent(IPlatformer plat, GroundedState what);

public enum GroundedState
{
    Neither,
    Grounded,
    Aerial
}

public interface IPlatformer
{

    event DelPlatformerConditionalEvent OnPlatformerGroundedStateChange;

    Transform transform
    {
        get;
    }

    Vector3 GetGroundedPositionOfLatestCheck();
    bool NeedExternalCheck
    {
        get;
    }

    void CheckPlatform();
    GroundedState IsGrounded();
}

[System.Serializable]
public class Platformer : FirstInterfaceGetter<IPlatformer> { }

public class PlatformerSetController : RigidbodyController , IPlatformer
{
    public struct CheckStat
    {
        public float time;

        public override string ToString()
        {
            return string.Format("({0})", time);
        }
    }

    [System.Serializable]
    public class PlatformerConditions : InterfaceGetter<IPlatformer> { }

    public PlatformerConditions conditions;
    //public PlatformerCondition test;

    public BoolUtils.BoolJoin howToJoinConditions;
    public GroundedState whatToTreatAsTrue;

    public event DelPlatformerConditionalEvent OnPlatformerGroundedStateChange;

    public Dictionary<IPlatformer, CheckStat> checks;

    private GroundedState _wasGrounded = GroundedState.Neither;

#if UNITY_EDITOR
    [TextArea]
    [SerializeField]
    [ReadOnly]
    private string _debugString;
#endif

    private bool IsValid(IPlatformer cond)
    {
        return cond != null && !cond.Equals(null) && !cond.Equals(this);
    }

    public IEnumerable<bool> AllConditions()
    {
#if UNITY_EDITOR
        _debugString = "";
#endif
        foreach (IPlatformer condition in conditions)
        {
            if (IsValid(condition))
            {
#if UNITY_EDITOR
                _debugString += string.Format("{0} is {1}. {2}\n", condition, condition.IsGrounded(), checks[condition]);
#endif
                yield return condition.IsGrounded() == whatToTreatAsTrue;
            }
        }
    }

    public void CheckPlatform()
    {
        for (int i = 0, len = conditions.Length; i < len; i++)
        {
            IPlatformer cond = conditions[i];
            if (IsValid(cond))
            {
                cond.CheckPlatform();
            }
        }
    }

    public GroundedState IsGrounded()
    {
        if (BoolUtils.Joinbools(howToJoinConditions, AllConditions())) return whatToTreatAsTrue;
        else return Negate(whatToTreatAsTrue);
    }

    public GroundedState Negate(GroundedState state)
    {
        switch (state)
        {
            case GroundedState.Grounded:
                return GroundedState.Aerial;
            case GroundedState.Aerial:
                return GroundedState.Grounded;
            default:
                return GroundedState.Neither;
        }
    }

    public Vector3 GetGroundedPositionOfLatestCheck()
    {
        IPlatformer chosen = conditions.Aggregate((x, y) =>
        {
            if (!IsValid(x)) return y;
            else if (!IsValid(y)) return x;
            else
            {
                var timeX = checks[x];
                var timeY = checks[y];
                return timeX.time >= timeY.time ? x : y;
            }
        });
        
        return chosen.GetGroundedPositionOfLatestCheck();
    }

    private void Awake()
    {
        checks = new Dictionary<IPlatformer, CheckStat>(2);
    }

    private void OnEnable()
    {
        checks.Clear();
        foreach(IPlatformer condition in conditions)
            if (IsValid(condition))
            {
                checks.Add(condition, new CheckStat()
                {
                    time = Time.time
                });
                condition.OnPlatformerGroundedStateChange += OnChildPlatformerStateChange;
            }
    }

    private void OnDisable()
    {
        foreach (IPlatformer condition in conditions)
            if (IsValid(condition))
                condition.OnPlatformerGroundedStateChange -= OnChildPlatformerStateChange;
    }

    private void OnChildPlatformerStateChange(IPlatformer plat, GroundedState what)
    {
        GroundedState grounded = IsGrounded();
        if(grounded != _wasGrounded)
        {
            var stat = checks[plat];
            stat.time = Time.time;
            checks[plat] = stat;
            OnPlatformerGroundedStateChange(this, grounded);
            _wasGrounded = grounded;
        }
    }

    public bool NeedExternalCheck
    {
        get
        {
            for (int i = 0, len = conditions.Length; i < len; i++)
            {
                IPlatformer cond = conditions[i];
                if (IsValid(cond))
                {
                    if (cond.NeedExternalCheck) return true;
                }
            }
            return false;
        }
    }
}

