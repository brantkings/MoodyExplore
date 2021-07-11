using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mood/Effect/Max Health", fileName = "Effect_MaxHealth_")]
public class ChangeMaxHealthMoodEffect : ChangeMoodPawnEffect
{
    public int amountChange = 10;

    public MaxLifeChange whenMaxLifeChanges = MaxLifeChange.Same;

    public enum MaxLifeChange
    {
        None,
        UpOnUp,
        DownOnDown,
        Same,
    }

    public bool neverKills = true;

    private MoodHealth Get(MoodPawn p)
    {
        return p.GetComponentInChildren<MoodHealth>();
    }

    protected override void AddChange(MoodPawn p)
    {
        ChangeMaxLife(p, amountChange);
    }

    protected override void RemoveChange(MoodPawn p)
    {
        ChangeMaxLife(p, -amountChange);
    }


    private void ChangeMaxLife(MoodPawn p, int amount)
    {
        MoodHealth h = Get(p);
        if (h != null)
        {
            h.GetMaxHealthParameter().SetBaseValue(h.GetMaxHealthParameter() + amount);
            int lifeChange = 0;
            switch (whenMaxLifeChanges)
            {
                case MaxLifeChange.None:
                    lifeChange = 0;
                    break;
                case MaxLifeChange.UpOnUp:
                    if (amount > 0)
                    {
                        lifeChange = -amount;
                    }
                    break;
                case MaxLifeChange.DownOnDown:
                    if (amount < 0)
                    {
                        lifeChange = -amount;
                    }
                    break;
                default:
                    lifeChange = -amount;
                    break;
            }
            if (neverKills)
            {
                if (h.Life <= lifeChange) lifeChange = 0;
            }
            if (lifeChange != 0)
                h.Damage(Health.MakeSimpleDamage(lifeChange, feedbacks: false));
        }
    }
}

