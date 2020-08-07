using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodIcon
{
    Texture2D GetIcon();
}

public abstract class MoodSkill : ScriptableObject, IMoodIcon
{
    [SerializeField]
    private Texture2D _icon;

    public Texture2D GetIcon()
    {
        return _icon;
    }

    public virtual bool CanExecute(MoodPawn pawn)
    {
        return true;
    }
    public abstract void Execute(MoodPawn pawn);
}
