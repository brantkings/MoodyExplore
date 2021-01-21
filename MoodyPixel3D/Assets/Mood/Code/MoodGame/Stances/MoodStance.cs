using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodStance : ScriptableObject
{
    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    private MoodReaction[] _reactions;

    public Sprite GetIcon()
    {
        return _icon;
    }

    public IEnumerable<MoodReaction> GetReactions()
    {
        foreach (MoodReaction react in _reactions) yield return react;
    }

}
