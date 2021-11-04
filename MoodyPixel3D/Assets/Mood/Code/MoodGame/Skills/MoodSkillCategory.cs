using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Skill/Skill Category", fileName = "SkillCategory_")]
public class MoodSkillCategory : ScriptableObject, IMoodSelectable
{
    [SerializeField]
    private string _name;

    [SerializeField]
    private Texture2D _icon;

    [SerializeField]
    private string _description;

    [SerializeField]
    private int _priority = 0;

    [SerializeField]
    private LHH.Unity.MorphableProperty<Color> _skillCommandColor;

    public string GetName(MoodPawn pawn)
    {
        return _name;
    }

    public string GetDescription()
    {
        return _description;
    }

    public Color? GetColor()
    {
        return _skillCommandColor.Get();
    }

    public Texture2D GetIcon()
    {
        return _icon;
    }

    public int GetPriority()
    {
        return _priority;
    }

    public void DrawCommandOption(MoodPawn pawn, MoodCommandOption option)
    {
        option.SetText(GetName(pawn), GetDescription(), 0f);
        option.SetStancePreview(null);
        option.SetFocusCost(0);
    }

    public bool CanBeShown(MoodPawn pawn)
    {
        return true;
    }

    public bool CanBePressed(MoodPawn pawn, Vector3 where)
    {
        return true;
    }
}
