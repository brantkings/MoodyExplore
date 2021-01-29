using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodStance : ScriptableObject
{
    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    private MoodReaction[] _reactions;

    [SerializeField]
    [Tooltip("Many skills actually check if the character is in neutral to be able to execute a skill. Inform if this stance will remove the character from neutral.")]
    private bool removeNeutralStance = false;

    public virtual bool IsNeutralStance()
    {
        return !removeNeutralStance;
    }

    public Sprite GetIcon()
    {
        return _icon;
    }

    public IEnumerable<MoodReaction> GetReactions()
    {
        foreach (MoodReaction react in _reactions) yield return react;
    }

}
