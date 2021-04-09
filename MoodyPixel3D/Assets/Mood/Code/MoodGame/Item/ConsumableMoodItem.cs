using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Item/Consumable", fileName = "Item_C_")]
public class ConsumableMoodItem : MoodItem
{
    public MoodSkill[] consumableSkills;

    public override bool CanUse(MoodPawn pawn, MoodInventory inventory)
    {
        return inventory;
    }

    public override void OnAdquire(MoodPawn pawn)
    {
    }

    public override void OnUse(MoodPawn pawn)
    {
        
    }
}
