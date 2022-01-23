using LHH.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodSelectable
{
    string name { get; }

    string GetName(MoodPawn pawn);

    Color? GetColor();

    void DrawCommandOption(MoodPawn pawn, MoodCommandOption option);

    bool CanBeShown(MoodPawn pawn);

    bool CanBePressed(MoodPawn pawn, Vector3 where);
}

public interface IMoodSelectableCustomPress : IMoodSelectable
{
    void Press();

    void Unpress();
}

public interface IMoodSkill
{
    void SetShowDirection(MoodPawn pawn, Vector3 direction);
    bool CanExecute(MoodPawn pawn, Vector3 where);
    bool CanBeShown(MoodPawn pawn);

    // A skill only enters play while other skill is executing if current plugout priority (got from pawn) is less than next plugin priority.
    int GetPluginPriority(MoodPawn pawn);
    
    /// <summary>
    /// Execute the skill. This should call ExecuteEffect() at some point and wait in real time for the return result. Override if you want to change delays, times, etc from the normal one. Dispatch execute event after you do the skill. Do not call base.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDirection">The direction to which the pawn is executing the skill.</param>
    /// <returns></returns>
    IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection);
    
    /// <summary>
    /// An skill can be interrupted at any time. Implement this to tell what should still happen, what shouldn't, kill tweens.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDirection">The direction to which the pawn is executing the skill.</param>
    /// <returns>The amount of time this should wait in real time.</returns>
    void Interrupt(MoodPawn pawn);
}


public abstract class MoodSkill : ScriptableObject, IMoodSelectable, IMoodSkill
{
    protected static int PRIORITY_CANCELLABLE = 1;
    protected static int PRIORITY_NOT_CANCELLABLE = 2;


    public enum ExecutionResult
    {
        Non_Applicable,
        Success,
        Failure
    }


    public delegate void MoodSkillEvent(MoodPawn pawn, Vector3 direction);
    public delegate void MoodSkillExecutionEvent(MoodPawn pawn, Vector3 direction, ExecutionResult success);



    public event MoodSkillExecutionEvent OnExecute;
    public event MoodSkillEvent OnPreview;

    [System.Serializable]
    public struct DirectionFixer
    {
        public float angleFromForward;
        public float coneAngle;

        [Tooltip("Not implemented yet.")]
        public bool mirrored;

        public float YAngleToSanitize(Vector3 direction, Vector3 characterLookingDirection)
        {
            float signedAngle = Vector3.SignedAngle(characterLookingDirection, direction, Vector3.up);
            float coneAngleDist = coneAngle * 0.5f;
            float angleDist = Mathf.DeltaAngle(signedAngle, angleFromForward);
            if(Mathf.Abs(angleDist) > coneAngleDist) //If outsideAngle
            {
                return LHH.Utils.NumberUtils.MinAbs(angleDist - coneAngleDist, angleDist + coneAngleDist);
            }
            else
            {
                return 0f;
            }
        }

        public static DirectionFixer LetAll
        {
            get
            {
                return new DirectionFixer()
                {
                    angleFromForward = 0f,
                    coneAngle = 180f
                };
            }
        }

        public Vector3 Sanitize(Vector3 vec, float angleToChange)
        {
            return Quaternion.Euler(0f, angleToChange, 0f) * vec;
        }

        public Vector3 Sanitize(Vector3 vec, Vector3 characterDirection)
        {
            return Sanitize(vec, YAngleToSanitize(vec, characterDirection));
        }
    }

    
    [SerializeField]
    private Texture2D _icon;
    
    [SerializeField]
    private string _name;

    [SerializeField]
    [TextArea()]
    private string _description;

    [SerializeField]
    private MoodSkillCategory _category;

    [SerializeField]
    private LHH.Unity.MorphableProperty<Color> _skillCommandColor;



    [Space()]
    [SerializeField] private MoodPawnCondition[] conditionsToUse;
    [SerializeField] private MoodPawnConditionUtils.JoinType joinConditionsType = MoodPawnConditionUtils.JoinType.All;
    [SerializeField]
    private bool onlyNeutralStance;
    [SerializeField]
    private MoodStance[] needs;
    [SerializeField]
    private bool consumeNeededStances;
    [SerializeField]
    private ActivateableMoodStance[] toConsume;
    [SerializeField]
    private MoodStance[] restrictions;
    [SerializeField]
    private DirectionFixer[] _possibleAngles;
    [SerializeField] private NumberUtils.NumberComparer<float>[] heightCheckers;

    [Space()]
    [SerializeField] private int _startupPriority;
    [SerializeField] private int _itemQuantityCost;
    [SerializeField] private int _freeFocusCost;

    [Space()]
    [SerializeField]
    private bool _lockCamera = true;
    [SerializeField]
    private bool lockMovement;

    [Space()]
    [SerializeField]
    private KeyCode  _shortcut;


    public KeyCode GetShortCut()
    {
        return _shortcut;
    }


    [Space()]
    [SerializeField]
    private bool _debug;

    public bool Debugging
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

    public bool CanBePressed(MoodPawn pawn, Vector3 direction)
    {
        return CanExecute(pawn, direction);
    }

    public bool IsBufferable(MoodPawn pawn)
    {
        return IsFocusAvailable(pawn);
    }

    public virtual Color? GetColor()
    {
        if (_category != null) return _category.GetColor();
        return _skillCommandColor.Get();
    }

    public virtual Texture2D GetIcon()
    {
        return _icon;
    }

    public virtual string GetName(MoodPawn pawn)
    {
        return _name;
    }

    public MoodSkillCategory GetCategory()
    {
        return _category;
    }

    public string GetDescription()
    {
        return _description;
    }

    private bool IsValidStanceSetup(MoodPawn pawn)
    {
        return pawn.HasAllStances(true, needs) && !pawn.HasAnyStances(false, restrictions);
    }

    public virtual void WriteOptionText(MoodPawn pawn, MoodCommandOption option)
    {
        option.SetText(GetName(pawn), GetDescription(), 0f);
    }

    public virtual void DrawCommandOption(MoodPawn pawn, MoodCommandOption option)
    {
        WriteOptionText(pawn, option);

        option.SetFocusCost(GetFocusCost());

        bool didChangeStance = false;
        foreach (MoodStance stance in GetStancesThatWillBeAdded())
        {
            option.SetStancePreview(stance.GetIcon(), false);
            didChangeStance = true;
            break;
        }
        if (!didChangeStance)
        {
            foreach (MoodStance stance in GetStancesThatWillBeRemoved())
            {
                option.SetStancePreview(stance.GetIcon(), true);
                didChangeStance = true;
                break;
            }
        }
        if (!didChangeStance) option.SetStancePreview(null);
    }

    private bool IsFocusAvailable(MoodPawn pawn)
    {
        IMindPawn mind = pawn as IMindPawn;
        if (mind != null && !mind.Equals(null))
        {
            return mind.GetAvailableFocusPoints() >= _freeFocusCost;
        }
        else return true;
    }

    private bool IsNeutralOK(MoodPawn pawn)
    {
        if (!onlyNeutralStance) return true;
        return pawn.IsInNeutralStance();
    }

    private bool IsHeightOK(MoodPawn pawn)
    {
        if (heightCheckers == null || heightCheckers.Length == 0) return true;
        float height = pawn.GetHeightFromGround();
        foreach (var check in heightCheckers) if (!check.Compare(height)) return false;
        return true;
    }

    private bool IsConditionsOK(MoodPawn pawn)
    {
        return MoodPawnConditionUtils.ConditionIsOk(conditionsToUse, pawn, joinConditionsType);
    }

    public virtual bool CanExecute(MoodPawn pawn, Vector3 where)
    {
        if(Debugging)
        {
            Debug.LogErrorFormat("{0} can do {1}? stance?{2} && focus?{3} && neutral?{4} && pawn?{5} && height{6}", pawn.name, name, IsValidStanceSetup(pawn), IsFocusAvailable(pawn), IsNeutralOK(pawn), pawn.CanUseSkill(this), IsHeightOK(pawn));
        }
        return IsValidStanceSetup(pawn) && IsFocusAvailable(pawn) && IsHeightOK(pawn) && IsNeutralOK(pawn) && pawn.CanUseSkill(this) && IsConditionsOK(pawn);
    }

    public virtual bool CanBeShown(MoodPawn pawn)
    {
        return IsValidStanceSetup(pawn) && IsConditionsOK(pawn);
    }

    /// <summary>
    /// Adhere to the skill's rules of direction! It's optional, but the player's skill should always do it.
    /// </summary>
    /// <param name="direction"></param>
    public virtual void SanitizeDirection(Vector3 lookingDirection, ref Vector3 direction)
    {
        SanitizeDirection(lookingDirection, ref direction, _possibleAngles);
    }

    protected void SanitizeDirection(Vector3 lookingDirection, ref Vector3 toSanitize, DirectionFixer[] fixers)
    {
        if (fixers != null && fixers.Length > 0)
        {
            float angleToChange = float.MaxValue;
            foreach (DirectionFixer angle in fixers)
            {
                angleToChange = NumberUtils.MinAbs(angleToChange, angle.YAngleToSanitize(toSanitize, lookingDirection));
            }
            if (angleToChange != 0f)
            {
                toSanitize = Quaternion.Euler(0f, angleToChange, 0f) * toSanitize;
            }
        }
    }


    /// <summary>
    /// Execute the skill. This should call ExecuteEffect() at some point and wait in real time for the return result. Override if you want to change delays, times, etc from the normal one. Dispatch execute event after you do the skill. Do not call base.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDirection">The direction to which the pawn is executing the skill.</param>
    /// <returns></returns>
    public virtual IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        (float duration, ExecutionResult executed) = ExecuteEffect(pawn, skillDirection);
        pawn.SetPlugoutPriority(_startupPriority); //By default, it is the startup priority.
        DispatchExecuteEvent(pawn, skillDirection, executed);
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }
        ConsumeStances(pawn);
    }

    public enum WillHaveTargetResult
    {
        NonApplicable,
        NotHaveTarget,
        WillHaveTarget,
    }

    /// <summary>
    /// If this skill has a concept of having a target, then see if with the current pawn and direction this will indeed have a valid target.
    /// </summary>
    /// <param name="pawn"></param>
    /// <param name="skillDirection"></param>
    /// <returns></returns>
    public abstract WillHaveTargetResult WillHaveTarget(MoodPawn pawn, Vector3 skillDirection, MoodUnitManager.DistanceBeats distanceSafety);

    public virtual IEnumerable<MoodStance> GetStancesThatWillBeAdded()
    {
        yield break;
    }

    public virtual IEnumerable<MoodStance> GetStancesThatWillBeRemoved()
    {
        foreach (MoodStance stance in toConsume) yield return stance;
        if (consumeNeededStances) foreach (MoodStance stance in needs) yield return stance;
    }

    protected void ConsumeStances(MoodPawn pawn)
    {
        foreach(var stance in toConsume) pawn.RemoveStance(stance);
        if (consumeNeededStances) foreach (ActivateableMoodStance stance in needs)
        {
            if(stance != null) pawn.RemoveStance(stance);
        }
    }

    protected void DispatchExecuteEvent(MoodPawn pawn, Vector3 skillDirection, ExecutionResult success)
    {
        OnExecute?.Invoke(pawn, skillDirection, success);
        pawn.UsedSkill(this, skillDirection, success);
    }

    public virtual IEnumerable<float> GetTimeIntervals(MoodPawn pawn, Vector3 skillDirection)
    {
         yield return 0f;
    }

    public virtual void GetProgress(MoodPawn pawn, Vector3 skillDirection, float timeElapsedSince, out float totalTime, out float currentProgressPercentage, out Color? currentProgressColor)
    {
        float now = 0f;
        currentProgressPercentage = 0f;
        currentProgressColor = null;
        totalTime = 0f;
        int index = 0;
        foreach (float elapsed in GetTimeIntervals(pawn, skillDirection))
        {
            float after = now + elapsed;
            totalTime = elapsed;
            index++;
            if (timeElapsedSince >= now && timeElapsedSince <= after)
            {
                if(index % 2 == 0)
                {
                    currentProgressPercentage = Mathf.InverseLerp(after, now, timeElapsedSince);
                }
                else
                {
                    currentProgressPercentage = Mathf.InverseLerp(now, after, timeElapsedSince);
                }
                currentProgressColor = null;
                break;
            }
            now = after;
        }
        //Debug.LogFormat("Returning total time {0}, current {1}, color {2}", totalTime, currentProgressPercentage, currentProgressColor);
    }

    public virtual void GetProgress(MoodPawn pawn, float timeElapsedSince, out float currentProgressPercentage, out Color? currentProgressColor)
    {
        currentProgressPercentage = 0f;
        currentProgressColor = null;
    }


    /// <summary>
    /// Execute the real effect! Return the duration of the effect that should be waited after.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDistance">The distance  to which the pawn is executing the skill. </param>
    /// <returns>The amount of time this should wait in real time and if the execution was considered a 'success' or not.</returns>

    protected abstract (float, ExecutionResult) ExecuteEffect(MoodPawn pawn, Vector3 skillDistance);

    protected static ExecutionResult MergeExecutionResult(ExecutionResult a, ExecutionResult b, bool priorityIsSuccess = true)
    {
        if (a == ExecutionResult.Non_Applicable) return b;
        else if (b == ExecutionResult.Non_Applicable) return a;

        if(priorityIsSuccess)
        {
            if (a == ExecutionResult.Success || b == ExecutionResult.Success) return ExecutionResult.Success;
            else return ExecutionResult.Failure;
        }
        else
        {
            if (a == ExecutionResult.Failure || b == ExecutionResult.Failure) return ExecutionResult.Failure;
            else return ExecutionResult.Success;
        }
    }

    protected static (float, ExecutionResult) MergeExecutionResult((float, ExecutionResult) a, (float, ExecutionResult) b)
    {
        return (a.Item1 + b.Item1, MergeExecutionResult(a.Item2, b.Item2, priorityIsSuccess: true));
    }

    public virtual void SetShowDirection(MoodPawn pawn, Vector3 direction)
    {
        OnPreview?.Invoke(pawn, direction);
    }

    public virtual void Interrupt(MoodPawn pawn)
    {
        //Only the mood pawn itself interrupts an skill ongoing, never the skill itself. This code should be related only to the skill.
    }

    public virtual int GetPluginPriority(MoodPawn pawn)
    {
        return _startupPriority;
    }

    public virtual bool NeedsCameraUpwards()
    {
        return _lockCamera;
    }

    public int GetFocusCost()
    {
        return _freeFocusCost;
    }

    public int GetItemCost()
    {
        return _itemQuantityCost;
    }
    
    //Property for visual show
    public virtual bool ImplementsRangeShow<T>()
    {
        return this is RangeShow<T>.IRangeShowPropertyGiver;
    }

    public virtual RangeShow<T>.IRangeShowPropertyGiver GetRangeShowProperty<T>()
    {
        return this as RangeShow<T>.IRangeShowPropertyGiver;
    }

    public virtual bool ShowsBark()
    {
        return true;
    }

}
