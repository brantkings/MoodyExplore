using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FlaggeableMoodStance : ActivateableMoodStance
{

    [Header("Flags")]
    [SerializeField]
    private MoodEffectFlag[] flags;

    public bool HasFlag(MoodEffectFlag flag)
    {
        return flags.Contains(flag);
    }
}
