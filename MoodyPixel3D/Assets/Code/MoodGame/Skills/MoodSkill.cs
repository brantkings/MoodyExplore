using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodSelectable
{
    Texture2D GetIcon();
    string GetName();
}

public interface IMoodSkill
{
    void SetShowDirection(MoodPawn pawn, Vector3 direction);
    bool CanExecute(MoodPawn pawn, Vector3 where);
    bool CanBeShown(MoodPawn pawn);
    
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
    public delegate void MoodSkillEvent(MoodPawn pawn, Vector3 direction);

    public event MoodSkillEvent OnExecute;
    public event MoodSkillEvent OnPreview;

    
    [SerializeField]
    private Texture2D _icon;
    
    [SerializeField]
    private string _name;

    
    [SerializeField]
    private MoodStance[] needs;
    [SerializeField]
    private bool consumeNeededStances;
    [SerializeField]
    private MoodStance[] toConsume;
    [SerializeField]
    private MoodStance[] restrictions;

    [SerializeField]
    private bool _lockCamera = true;
    [SerializeField]
    private bool lockMovement;

    [SerializeField]
    private KeyCode  _shortcut;
    public KeyCode GetShortCut()
    {
        return _shortcut;
    }

    public Texture2D GetIcon()
    {
        return _icon;
    }

    public string GetName()
    {
        return _name;
    }

    private bool IsValidStanceSetup(MoodPawn pawn)
    {
        return pawn.HasAllStances(true, needs) && !pawn.HasAnyStances(false, restrictions);
    }

    public virtual bool CanExecute(MoodPawn pawn, Vector3 where)
    {
        return IsValidStanceSetup(pawn);
    }

    public virtual bool CanBeShown(MoodPawn pawn)
    {
        return IsValidStanceSetup(pawn);
    }

    /// <summary>
    /// Execute the skill. This should call ExecuteEffect() at some point and wait in real time for the return result. Override if you want to change delays, times, etc from the normal one. Dispatch execute event after you do the skill. Do not call base.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDirection">The direction to which the pawn is executing the skill.</param>
    /// <returns></returns>
    public virtual IEnumerator ExecuteRoutine(MoodPawn pawn, Vector3 skillDirection)
    {
        Debug.LogFormat("Executing {0}", this);
        pawn.MarkUsingSkill(this);
        float duration = ExecuteEffect(pawn, skillDirection);
        DispatchExecuteEvent(pawn, skillDirection);
        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
        }
        ConsumeStances(pawn);
        pawn.UnmarkUsingSkill(this);
    }

    protected void ConsumeStances(MoodPawn pawn)
    {
        foreach(var stance in toConsume) pawn.RemoveStance(stance);
        if(consumeNeededStances) foreach(var stance in needs) pawn.RemoveStance(stance);
    }

    protected void DispatchExecuteEvent(MoodPawn pawn, Vector3 skillDirection)
    {
        OnExecute?.Invoke(pawn, skillDirection);
        pawn.UsedSkill(this, skillDirection);
    }

    /// <summary>
    /// Execute the real effect! Return the duration of the effect that should be waited after.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDirection">The direction to which the pawn is executing the skill.</param>
    /// <returns>The amount of time this should wait in real time.</returns>
    protected abstract float ExecuteEffect(MoodPawn pawn, Vector3 skillDirection);

    public virtual void SetShowDirection(MoodPawn pawn, Vector3 direction)
    {
        OnPreview?.Invoke(pawn, direction);
    }

    public virtual void Interrupt(MoodPawn pawn) 
    {
        pawn.InterruptedSkill(this);
    }

    public virtual bool NeedsCameraUpwards()
    {
        return _lockCamera;
    }

}
