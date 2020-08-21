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
    IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection);
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

    public Texture2D GetIcon()
    {
        return _icon;
    }

    public string GetName()
    {
        return _name;
    }

    public virtual bool CanExecute(MoodPawn pawn, Vector3 where)
    {
        return true;
    }

    /// <summary>
    /// Execute the skill. This should call ExecuteEffect() at some point and wait in real time for the return result. Override if you want to change delays, times, etc from the normal one. Dispatch execute event after you do the skill. Do not call base.
    /// </summary>
    /// <param name="pawn">The pawn that is executing the skill.</param>
    /// <param name="skillDirection">The direction to which the pawn is executing the skill.</param>
    /// <returns></returns>
    public virtual IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection)
    {
        float duration = ExecuteEffect(pawn, skillDirection);
        DispatchExecuteEvent(pawn, skillDirection);
        if (duration > 0f)
        {
            yield return new WaitForSecondsRealtime(duration);
        }
    }

    protected void DispatchExecuteEvent(MoodPawn pawn, Vector3 skillDirection)
    {
        OnExecute?.Invoke(pawn, skillDirection);
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
}
