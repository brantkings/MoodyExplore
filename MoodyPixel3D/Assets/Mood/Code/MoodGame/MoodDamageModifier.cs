using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DMGMOD_", menuName ="Mood/Damage/Damage Modifier")]
public class MoodDamageModifier : ScriptableObject
{
    public float damageMultiplier = 1f;
    public float damageAdd = 0f;
    public BackToInt backToInt = BackToInt.Floor;

    public enum BackToInt
    {
        Floor,
        Ceil,
        Round
    }

    public void ModifyDamage(ref DamageInfo info)
    {
        info.damage = GoBackToInt(info.damage * damageMultiplier + damageAdd);
    }

    private int GoBackToInt(float n)
    {
        switch (backToInt)
        {
            case MoodDamageModifier.BackToInt.Floor:
                return Mathf.FloorToInt(n);
            case MoodDamageModifier.BackToInt.Ceil:
                return Mathf.CeilToInt(n);
            case MoodDamageModifier.BackToInt.Round:
                return Mathf.RoundToInt(n);
            default:
                return Mathf.FloorToInt(n);
        }
    }
}
