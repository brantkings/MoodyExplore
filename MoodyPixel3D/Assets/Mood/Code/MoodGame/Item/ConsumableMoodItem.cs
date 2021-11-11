using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Mood/Item/Consumable", fileName = "Item_C_")]
public class ConsumableMoodItem : MoodItem
{
    public MoodSkill[] consumableSkills;

    public override bool CanUse(MoodPawn pawn, IMoodInventory inventory)
    {
        return !inventory.Equals(null);
    }

    public override void OnAdquire(MoodPawn pawn)
    {
    }


    public override IEnumerable<MoodSkill> GetSkills()
    {
        return consumableSkills;
    }

    public override string GetItemStatusDescription(in MoodItemInstance.Properties properties, in bool equipped)
    {
        string str = "";
        if (equipped) str += "<color=green>Eqpd</color>\n";
        if (properties.quantity != 0) AddStatusText(ref str, "x", properties.quantity);
        return str;
    }
}
