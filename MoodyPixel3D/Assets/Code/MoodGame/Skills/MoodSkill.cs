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
    public abstract IEnumerator Execute(MoodPawn pawn, Vector3 skillDirection);

    public virtual void SetShowDirection(MoodPawn pawn, Vector3 direction)
    {
    }
}
