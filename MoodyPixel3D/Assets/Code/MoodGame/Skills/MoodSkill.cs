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
    bool CanExecute(MoodPawn pawn, Vector3 where);
    void Execute(MoodPawn pawn, Vector3 skillDirection);
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
    public abstract void Execute(MoodPawn pawn, Vector3 skillDirection);
}
